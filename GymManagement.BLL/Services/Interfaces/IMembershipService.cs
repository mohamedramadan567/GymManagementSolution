using GymManagement.BLL.Common;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using GymManagement.BLL.ViewModels.SessionViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Interfaces
{
    public interface IMembershipService
    {
        Task<IEnumerable<MembershipViewModel>> GetAllMembershipsAsync(CancellationToken ct = default);
        Task<Result> CreateMembershipAsync(CreateMembershipViewModel model, CancellationToken ct = default);
        Task<Result> DeleteActiveMembershipAsync(int memberId, CancellationToken ct = default);
        Task<IEnumerable<MemberSelectViewModel>> GetMembersForDropDownAsync(CancellationToken ct = default);
        Task<IEnumerable<PlanSelectViewModel>> GetPlansForDropDownAsync(CancellationToken ct = default);
    }
}
