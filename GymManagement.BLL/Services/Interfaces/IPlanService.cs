using GymManagement.BLL.Common;
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
        Task<Result<IEnumerable<PlanViewModel>>> GetAllPlansAsync(CancellationToken ct = default);
        Task<Result<PlanViewModel>> GetPlanByIdAsync(int planId, CancellationToken ct = default);
        Task<Result<PlanToUpdateViewModel>> GetPlanToUpdateAsync(int planId, CancellationToken ct = default);
        Task<Result> UpdatePlanDetailsAsync(int planId, PlanToUpdateViewModel model, CancellationToken ct = default);
        Task<Result> ToggleActivationAsync(int planId, CancellationToken ct = default);
        Task<Result> HasActiveMembershipsAsync(int planId, CancellationToken ct = default);
    }
}
