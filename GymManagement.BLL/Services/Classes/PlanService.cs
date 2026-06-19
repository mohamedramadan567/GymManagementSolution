using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.PlanViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result<IEnumerable<PlanViewModel>>> GetAllPlansAsync(CancellationToken ct = default)
        {
            var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync(ct: ct);

            if (!plans.Any())
                return Result<IEnumerable<PlanViewModel>>.NotFound("No plans found");

            var mappedPlans = _mapper.Map<IEnumerable<Plan>, IEnumerable<PlanViewModel>>(plans);
            return Result<IEnumerable<PlanViewModel>>.OK(mappedPlans);
        }

        public async Task<Result<PlanViewModel>> GetPlanByIdAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);

            if (plan is null)
                return Result<PlanViewModel>.NotFound("Plan Not Found");

            var mappedPlan = _mapper.Map<Plan, PlanViewModel>(plan);
            return Result<PlanViewModel>.OK(mappedPlan);
        }

        public async Task<Result<PlanToUpdateViewModel>> GetPlanToUpdateAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            if (plan is null)
                return Result<PlanToUpdateViewModel>.NotFound("Plan Not Found");

            if (!plan.IsActive)
                return Result<PlanToUpdateViewModel>.Fail("Cannot update an inactive plan", ResultKind.ValidationFailed);

            var hasActiveResult = await HasActiveMembershipsAsync(planId, ct);
            if (!hasActiveResult.success)
                return Result<PlanToUpdateViewModel>.Fail(hasActiveResult.error ?? "Cannot update plan with active memberships", ResultKind.ValidationFailed);

            var mappedPlan = _mapper.Map<Plan, PlanToUpdateViewModel>(plan);
            return Result<PlanToUpdateViewModel>.OK(mappedPlan);
        }

        public async Task<Result> ToggleActivationAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            if (plan is null)
                return Result.NotFound("Plan Not Found");

            if (plan.IsActive)
            {
                var hasActiveResult = await HasActiveMembershipsAsync(planId, ct);
                if (!hasActiveResult.success)
                    return Result.Validation(hasActiveResult.error ?? "Cannot deactivate plan with active memberships");
            }

            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Plan>().Update(plan);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to toggle plan activation");
        }

        public async Task<Result> UpdatePlanDetailsAsync(int planId, PlanToUpdateViewModel model, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            if (plan is null)
                return Result.NotFound("Plan Not Found");

            var hasActiveResult = await HasActiveMembershipsAsync(planId, ct);
            if (!hasActiveResult.success)
                return Result.Validation(hasActiveResult.error ?? "Cannot update plan with active memberships");

            _mapper.Map(model, plan);
            plan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Plan>().Update(plan);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to update plan details");
        }

        #region Helper Method
        public async Task<Result> HasActiveMembershipsAsync(int planId, CancellationToken ct = default)
        {
            var hasActive = await _unitOfWork.GetRepository<MemberShip>().AnyAsync(x => x.PlanId == planId && x.EndDate > DateTime.Now, ct);

            return hasActive
                ? Result.Fail("Plan has active memberships")
                : Result.OK();
        }
        #endregion
    }
}
