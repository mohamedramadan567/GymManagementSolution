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
        private readonly IGenericRepository<Plan> _planRepository;
        private readonly IGenericRepository<MemberShip> _membershipRepository;

        public PlanService(IGenericRepository<Plan> planRepository,
                           IGenericRepository<MemberShip> membershipRepository)
        {
            _planRepository = planRepository;
            _membershipRepository = membershipRepository;
        }
        public async Task<IEnumerable<PlanViewModel>> GetAllPlansAsync(CancellationToken ct = default)
        {
            var plans = await _planRepository.GetAllAsync(ct: ct);

            return plans.Select(p => new PlanViewModel()
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                DurationDays = p.DurationDays,
                IsActive = p.IsActive 
            });

        }

        public async Task<PlanViewModel?> GetPlanByIdAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId, ct);

            if (plan is null) return null;

            return new PlanViewModel()
            {
                Name = plan.Name,
                Price = plan.Price,
                Description = plan.Description,
                DurationDays = plan.DurationDays,
                IsActive = plan.IsActive
            };
        }

        public async Task<PlanToUpdateViewModel?> GetPlanToUpdateAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId, ct);
            if (plan is null || !plan.IsActive) return null;

            bool result = await HasActiveMembershipsAsync(planId, ct);
            if (result) return null;

            return new PlanToUpdateViewModel()
            {
                PlanName = plan.Name,
                Description = plan.Description,
                DurationDays = plan.DurationDays,
                Price = plan.Price
            };
        }


        public async Task<bool> ToggleActivationAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId, ct);
            if (plan is null) return false;

            if (plan.IsActive && await HasActiveMembershipsAsync(planId, ct)) return false;

            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.Now;
            var result = await _planRepository.UpdateAsync(plan, ct);
            return result > 0;
        }

        public async Task<bool> UpdatePlanDetailsAsync(int planId, PlanToUpdateViewModel model, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId, ct);
            if (plan is null) return false;

            if (await HasActiveMembershipsAsync(planId, ct)) return false;

            plan.Price = model.Price;
            plan.DurationDays = model.DurationDays;
            plan.Description = model.Description;
            plan.UpdatedAt = DateTime.Now;

            var result = await _planRepository.UpdateAsync(plan, ct);
            return result > 0;
        }

        #region Helper Method
        public async Task<bool> HasActiveMembershipsAsync(int planId, CancellationToken ct = default)
        {
            return await _membershipRepository.AnyAsync(x => x.PlanId == planId && x.EndDate > DateTime.Now);
        }
        #endregion
    }
}
