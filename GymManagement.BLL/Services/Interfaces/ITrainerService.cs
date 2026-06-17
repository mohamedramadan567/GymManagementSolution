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
        public Task<IEnumerable<TrainerViewModel>> GetAllTrainersAsync(CancellationToken ct = default);
        public Task<bool> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default);
        public Task<TrainerViewModel?> GetTrainerDetailsByIdAsync(int trainerId, CancellationToken ct = default);
        public Task<TrainerToUpdateViewModel?> GetTrainerToUpdateAsync(int trainerId, CancellationToken ct = default);
        public Task<bool> UpdateTrainerDetailsAsync(int id, TrainerToUpdateViewModel model, CancellationToken ct = default);
        public Task<bool> RemoveTrainerAsync(int trainerId, CancellationToken ct = default);

    }
}
