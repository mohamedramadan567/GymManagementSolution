using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MemberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            //Check Email
            var emailExit = await _unitOfWork.GetRepository<Member>().AnyAsync(x => x.Email == model.Email, ct);
            //Check Phone
            var phoneExit = await _unitOfWork.GetRepository<Member>().AnyAsync(x => x.Phone == model.Phone, ct);
            //if Email or Phone Exists return false
            if (emailExit || phoneExit) return false;
            //else add member
            var member = new Member()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = new Address()
                {
                    BuildingNumber = model.BuildingNumber,
                    City = model.City,
                    Street = model.Street
                },
                HealthRecord = new HealthRecord()
                {
                    BloodType = model.HealthRecordViewModel.BloodType,
                    Weight = model.HealthRecordViewModel.Weight,
                    Height = model.HealthRecordViewModel.Height,
                    Note = model.HealthRecordViewModel.Note
                }
            };


            _unitOfWork.GetRepository<Member>().Add(member); // Add local
            var added = await _unitOfWork.SaveChangesAsync(ct);
            return added > 0;
        }


        public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct)
        {
            var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(ct: ct);

            if (!members.Any()) return [];

            var memberViewModel = members.Select(m => new MemberViewModel()
            {
                Id = m.Id,
                Photo = m.Photo,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Gender = m.Gender.ToString()
            });

            return memberViewModel;
        }

        public async Task<MemberViewModel?> GetMemberDetailsByIdAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);

            if (member == null) return null;

            var model = new MemberViewModel()
            {
                Name = member.Name,
                Email = member.Email,
                Phone = member.Phone,
                Gender = member.Gender.ToString(),
                DateOfBirth = member.DateOfBirth.ToShortDateString(),
                Address = $"{member.Address.BuildingNumber} - {member.Address.Street} - {member.Address.City}",
            };

            var activeMembership = await _unitOfWork.GetRepository<MemberShip>().FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate > DateTime.Now);

            if(activeMembership is not null)
            {
                var activePlan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(activeMembership.PlanId, ct);
                model.PlanName = activePlan?.Name;
                model.MembershipStartDate = activeMembership.CreatedAt.ToString();
                model.MembershipEndDate = activeMembership.EndDate.ToString();
            }

            return model;

        }

        public async Task<HealthRecordViewModel?> GetMemberHealthRecordByIdAsync(int memberId, CancellationToken ct = default)
        {
            var healthRecord = await _unitOfWork.GetRepository<HealthRecord>().FirstOrDefaultAsync(x => x.MemberId == memberId, ct: ct);
            if (healthRecord is null) return null;
            return new HealthRecordViewModel()
            {
                Height = healthRecord.Height,
                Weight = healthRecord.Weight,
                BloodType = healthRecord.BloodType,
                Note = healthRecord.Note
            };
        }

        public async Task<MemberToUpdateViewModel?> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);

            if (member is null) return null;

            var model = new MemberToUpdateViewModel()
            {
                BuildingNumber = member.Address.BuildingNumber,
                City = member.Address.City,
                Street = member.Address.Street,
                Email = member.Email,
                Name = member.Name,
                Phone = member.Phone,
                Photo = member.Photo
            };

            return model;
        }

        public async Task<bool> RemoveMemberAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);

            if (member == null) return false;

            var hasFutureBookings = await _unitOfWork.GetRepository<Booking>().AnyAsync(m => m.MemberId == memberId && m.Session.StartDate > DateTime.Now);

            if (hasFutureBookings) return false;

            _unitOfWork.GetRepository<Member>().Delete(member); //Delete Local
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;

        }

        public async Task<bool> UpdateMemberDetailsAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(id, ct);
            if (member == null) return false;

            var EmailExist = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Email == member.Email && m.Id != id);
            var PhoneExist = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Phone == member.Phone && m.Id != id);

            if (EmailExist || PhoneExist) return false;

            member.Address.City =  model.City;
            member.Email = model.Email;
            member.Address.Street = model.Street;
            member.Address.BuildingNumber = model.BuildingNumber;
            member.Phone = model.Phone;
            member.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Member>().Update(member);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }
    }
}
