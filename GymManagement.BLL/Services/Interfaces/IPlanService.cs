using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.BLL.ViewModels.PlanViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Interfaces
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanViewModel>> GetAllPlansAsync(CancellationToken ct = default);
        Task<PlanViewModel?> GetPlanByIdAsync(int planId, CancellationToken ct = default);
        Task<PlanToUpdateViewModel?> GetPlanToUpdateAsync(int planId, CancellationToken ct = default);
        Task<bool> UpdatePlanDetailsAsync(int planId, PlanToUpdateViewModel model, CancellationToken ct = default);
        Task<bool> ToggleActivationAsync(int planId, CancellationToken ct = default);
        Task<bool> HasActiveMembershipsAsync(int planId, CancellationToken ct = default);
    }
}
