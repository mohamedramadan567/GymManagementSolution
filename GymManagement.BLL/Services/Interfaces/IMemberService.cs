using GymManagement.BLL.ViewModels.MemberViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Interfaces
{
    public interface IMemberService
    {
        Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct);

        Task<bool> CreateMemberAsync(CreateMemberViewModel member, CancellationToken ct = default);

        Task<MemberViewModel?> GetMemberDetailsByIdAsync(int memberId, CancellationToken ct = default);

        Task<HealthRecordViewModel?> GetMemberHealthRecordByIdAsync(int memberId, CancellationToken ct = default);

        Task<MemberToUpdateViewModel?> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default);

        Task<bool> UpdateMemberDetailsAsync(int memberId, MemberToUpdateViewModel model, CancellationToken ct = default);
    }
}
