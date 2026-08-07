using GymManagement.BLL.Common;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Interfaces
{
    public interface ITrainerService
    {
        public Task<Result<IEnumerable<TrainerViewModel>>> GetAllTrainersAsync(string? search = null, CancellationToken ct = default);
        public Task<Result> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default);
        public Task<Result<TrainerViewModel>> GetTrainerDetailsByIdAsync(int trainerId, CancellationToken ct = default);
        public Task<Result<TrainerToUpdateViewModel>> GetTrainerToUpdateAsync(int trainerId, CancellationToken ct = default);
        public Task<Result> UpdateTrainerDetailsAsync(int id, TrainerToUpdateViewModel model, CancellationToken ct = default);
        public Task<Result> RemoveTrainerAsync(int trainerId, CancellationToken ct = default);

    }
}
