using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrainerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            this._mapper = mapper;
        }
        public async Task<Result> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default)
        {
            var emailExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Email == model.Email, ct);
            var phoneExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Phone == model.Phone, ct);

            if (emailExist || phoneExist)
                return Result.Validation("Email or Phone number already exists");

            var trainer = _mapper.Map<CreateTrainerViewModel, Trainer>(model);

            _unitOfWork.GetRepository<Trainer>().Update(trainer); //Update local
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to create trainer");
        }

        public async Task<Result<IEnumerable<TrainerViewModel>>> GetAllTrainersAsync(CancellationToken ct = default)
        {
            var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(ct: ct);

            if (!trainers.Any())
                return Result<IEnumerable<TrainerViewModel>>.NotFound("No trainers found");

            var model = _mapper.Map<IEnumerable<Trainer>, IEnumerable<TrainerViewModel>>(trainers);
            return Result<IEnumerable<TrainerViewModel>>.OK(model);

        }

        public async Task<Result<TrainerViewModel>> GetTrainerDetailsByIdAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId, ct);

            if (trainer == null)
                return Result<TrainerViewModel>.NotFound("Trainer Not Found");

            var model = _mapper.Map<Trainer, TrainerViewModel>(trainer);
            return Result<TrainerViewModel>.OK(model);

        }

        public async Task<Result<TrainerToUpdateViewModel>> GetTrainerToUpdateAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId, ct);
            if (trainer == null) return Result<TrainerToUpdateViewModel>.NotFound("Trainer Not Found");

            var model = _mapper.Map<Trainer, TrainerToUpdateViewModel>(trainer);
            return Result<TrainerToUpdateViewModel>.OK(model);
        }

        public async Task<Result> RemoveTrainerAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId, ct);

            if (trainer == null)
                return Result.NotFound("Trainer Not Found");

            var hasFutureSessions = await _unitOfWork.GetRepository<Session>()
                .AnyAsync(s => s.TrainerId == trainerId && s.StartDate > DateTime.Now);

            if (hasFutureSessions)
                return Result.Validation("Cannot remove trainer with future sessions booked");

            _unitOfWork.GetRepository<Trainer>().Delete(trainer);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to remove trainer");
        }

        public async Task<Result> UpdateTrainerDetailsAsync(int id, TrainerToUpdateViewModel model, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(id, ct);

            if (trainer == null)
                return Result.NotFound("Trainer Not Found");

            var emailExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Email == model.Email && t.Id != id, ct);
            var phoneExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Phone == model.Phone && t.Id != id, ct);

            if (emailExist || phoneExist)
                return Result.Validation("Email or Phone number already exists for another trainer");

            _mapper.Map(model, trainer);
            trainer.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Trainer>().Update(trainer);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to update trainer details");
        }
    }
}
