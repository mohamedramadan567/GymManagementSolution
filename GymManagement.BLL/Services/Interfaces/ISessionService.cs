using GymManagement.BLL.Common;
using GymManagement.BLL.ViewModels.SessionViewModels;
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
    }
}
