using GymManagement.BLL.Common;
using GymManagement.BLL.ViewModels.SessionViewModels;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Interfaces
{
    public interface ISessionService
    {
        Task<Result<IEnumerable<SessionViewModel>>> GetAllSessionsAsync(CancellationToken ct = default);
        Task<Result> CreateSessionAsync(CreateSessionViewModel model, CancellationToken ct = default);
        Task<Result<IEnumerable<TrainerSelectViewModel>>> GetTrainersForDropDownAsync(CancellationToken ct = default);
        Task<Result<IEnumerable<CategorySelectViewModel>>> GetCategoriesForDropDownAsync(CancellationToken ct = default);
        Task<Result<SessionViewModel>> GetSessionDetailsByIdAsync(int sessionId, CancellationToken ct = default);
        public Task<Result<SessionToUpdateViewModel>> GetSessionToUpdateAsync(int trainerId, CancellationToken ct = default);
        public Task<Result> UpdateSessionDetailsAsync(int id, SessionToUpdateViewModel model, CancellationToken ct = default);
        Task<Result> RemoveSessionAsync(int sessionId, CancellationToken ct = default);
    }
}
