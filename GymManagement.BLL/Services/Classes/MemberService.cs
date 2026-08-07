using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Attachment;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {

        //Not Completed Continue Refactoring to Result Pattern

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;

        public MemberService(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
             _attachmentService = attachmentService;
        }

        public async Task<Result> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            //Check Email
            var emailExit = await _unitOfWork.GetRepository<Member>().AnyAsync(x => x.Email == model.Email, ct);
            //Check Phone
            var phoneExit = await _unitOfWork.GetRepository<Member>().AnyAsync(x => x.Phone == model.Phone, ct);
            //if Email or Phone Exists return false
            if (emailExit || phoneExit) return Result.Validation("Email or phone already exist try anthor one");

            //Upload Photo
            var storedPhotoName = await _attachmentService.UploadAsync(model.PhotoFile.OpenReadStream(), model.PhotoFile.FileName, "MembersPhoto", ct);
            if (string.IsNullOrWhiteSpace(storedPhotoName.value)) return Result.Fail("Failed to upload Photo");
            //else add member
            var member = _mapper.Map<CreateMemberViewModel, Member>(model);
            member.Photo = storedPhotoName.value;

            _unitOfWork.GetRepository<Member>().Add(member); // Add local
            var result = await _unitOfWork.SaveChangesAsync(ct);
            if(result > 0)
            {
                return Result.OK();
            }
            else
            {
                //Delete Uploaded Photo
                _attachmentService.Delete(storedPhotoName.value, "MembersPhoto");
                return Result.Fail("Failed to Create Member");
            }
            
        }


        public async Task<Result<IEnumerable<MemberViewModel>>> GetAllMembersAsync(string? search = null, CancellationToken ct = default)
        {
            // Build filter for search (Name, Email, Phone) - case insensitive partial match
            Expression<Func<Member, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                filter = m => m.Name.ToLower().Contains(q) || m.Email.ToLower().Contains(q) || m.Phone.ToLower().Contains(q);
            }

            var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(filter, ct: ct);

            if (!members.Any())
                return Result<IEnumerable<MemberViewModel>>.NotFound("No members found");

            var memberViewModel = _mapper.Map<IEnumerable<Member>, IEnumerable<MemberViewModel>>(members);
            //foreach (var member in memberViewModel)
            //{
            //    member.Photo = _attachmentService.GetFile()
            //}

            return Result<IEnumerable<MemberViewModel>>.OK(memberViewModel);
        }

        public async Task<Result<MemberViewModel>> GetMemberDetailsByIdAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);

            if (member == null)
                return Result<MemberViewModel>.NotFound("Member Not Found");

            var model = _mapper.Map<Member, MemberViewModel>(member);

            var activeMembership = await _unitOfWork.GetRepository<MemberShip>()
                .FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate > DateTime.Now);

            if (activeMembership is not null)
            {
                var activePlan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(activeMembership.PlanId, ct);
                model.PlanName = activePlan?.Name;
                model.MembershipStartDate = activeMembership.CreatedAt.ToString();
                model.MembershipEndDate = activeMembership.EndDate.ToString();
            }

            return Result<MemberViewModel>.OK(model);
        }

        public async Task<Result<HealthRecordViewModel>> GetMemberHealthRecordByIdAsync(int memberId, CancellationToken ct = default)
        {
            var healthRecord = await _unitOfWork.GetRepository<HealthRecord>().FirstOrDefaultAsync(x => x.MemberId == memberId, ct: ct);

            if (healthRecord is null)
                return Result<HealthRecordViewModel>.NotFound("Health Record Not Found");

            var model = _mapper.Map<HealthRecord, HealthRecordViewModel>(healthRecord);
            return Result<HealthRecordViewModel>.OK(model);
        }

        public async Task<Result<MemberToUpdateViewModel>> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);

            if (member is null)
                return Result<MemberToUpdateViewModel>.NotFound("Member Not Found");

            var model = _mapper.Map<Member, MemberToUpdateViewModel>(member);

            return Result<MemberToUpdateViewModel>.OK(model);
        }

        public async Task<Result> RemoveMemberAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);

            if (member == null) return Result.NotFound("Member Not Found");

            var hasFutureBookings = await _unitOfWork.GetRepository<Booking>()
                .AnyAsync(m => m.MemberId == memberId && m.Session.StartDate > DateTime.Now);

            if (hasFutureBookings)
                return Result.Validation("Cannot remove member with future bookings");

            _unitOfWork.GetRepository<Member>().Delete(member); //Delete Local
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to remove member");
        }

        public async Task<Result> UpdateMemberDetailsAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(id, ct);
            if (member == null) return Result.NotFound("Member Not Found");

            var EmailExist = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Email == member.Email && m.Id != id);
            var PhoneExist = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Phone == member.Phone && m.Id != id);

            if (EmailExist || PhoneExist) return Result.Validation("Email and Phone Must be Exist");

            _mapper.Map(model, member);
            member.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Member>().Update(member);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.OK() : Result.Fail("Failed to Update Member");
        }
    }
}
