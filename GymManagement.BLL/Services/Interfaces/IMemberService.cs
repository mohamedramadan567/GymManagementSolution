using GymManagement.BLL.Common;
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
        Task<Result<IEnumerable<MemberViewModel>>> GetAllMembersAsync(string? search = null, CancellationToken ct = default);

        Task<Result> CreateMemberAsync(CreateMemberViewModel member, CancellationToken ct = default);

        Task<Result<MemberViewModel>?> GetMemberDetailsByIdAsync(int memberId, CancellationToken ct = default);

        Task<Result<HealthRecordViewModel>> GetMemberHealthRecordByIdAsync(int memberId, CancellationToken ct = default);

        Task<Result<MemberToUpdateViewModel>?> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default);

        Task<Result> UpdateMemberDetailsAsync(int memberId, MemberToUpdateViewModel model, CancellationToken ct = default);

        Task<Result> RemoveMemberAsync(int memberId, CancellationToken ct = default);
    }
}
