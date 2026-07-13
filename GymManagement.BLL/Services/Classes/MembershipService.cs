using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class MembershipService : IMembershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MembershipService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MembershipViewModel>> GetAllMembershipsAsync(CancellationToken ct = default)
        {
            var memberships = await _unitOfWork.MembershipRepository.GetMembershipsWithMembersAndPlansAsync(m => m.EndDate > DateTime.UtcNow, ct);
            return _mapper.Map<IEnumerable<MembershipViewModel>>(memberships);
        }
        public async Task<Result> CreateMembershipAsync(CreateMembershipViewModel model, CancellationToken ct = default)
        {
            //1.A membership can only be created if the member exists in the system.
            var memberExits = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Id == model.MemberId, ct);
            if (!memberExits)
                return Result.NotFound("Member Not Found");
            //2.A membership can only be created if the plan exists in the system.
            var planExists = await _unitOfWork.GetRepository<Plan>().AnyAsync(p => p.Id == model.PlanId, ct);
            if (!planExists)
                return Result.NotFound("Plan Not Found");

            //3.A member cannot have more than one Active membership at the same time.
            var hasActiveMembership = await _unitOfWork.MembershipRepository.AnyAsync(m => m.MemberId == model.MemberId && m.EndDate > DateTime.UtcNow, ct);
            if (hasActiveMembership)
                return Result.Fail("Member has already an active membership");

            //4.Only active plans can be assigned
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(model.PlanId, ct);
            if (!plan.IsActive)
                return Result.Fail("Plan is not active");

            //5.When a membership is created, its EndDate is automatically calculated based
            //on the plan duration.
            var membershipEntity = new MemberShip
            {
                MemberId = model.MemberId,
                PlanId = model.PlanId,
                CreatedAt = DateTime.UtcNow,
                EndDate = (model.StartDate ?? DateTime.UtcNow).AddDays(plan.DurationDays)
            };

            //6.Membership status is computed: "Active" if EndDate > Now, else "Expired"
            //already Computed

            _unitOfWork.MembershipRepository.Add(membershipEntity);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.OK() : Result.Fail("Failed to Create Membership");

        }

        public async Task<IEnumerable<MemberSelectViewModel>> GetMembersForDropDownAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<MemberSelectViewModel>>(members);
        }

        public async Task<IEnumerable<PlanSelectViewModel>> GetPlansForDropDownAsync(CancellationToken ct = default)
        {
            var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<PlanSelectViewModel>>(plans);
        }

        public async Task<Result> DeleteActiveMembershipAsync(int memberId, CancellationToken ct = default)
        {
            //1 - Cancellation Delete Memberships For Member On This Plan 

            //2 - A membership can only be deleted if it is Active.
            var activeMembership = await _unitOfWork.MembershipRepository.FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate > DateTime.UtcNow, true, ct);
            if (activeMembership is null) return Result.Fail("Active membership not found for the member", ResultKind.NotFound);

            _unitOfWork.MembershipRepository.Delete(activeMembership);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.OK() : Result.Fail("Failed to Delete Membership");
        }

    }
}
