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
        private readonly IGenericRepository<Trainer> _trainerRepository;
        private readonly IGenericRepository<Session> _sessionRepository;

        public TrainerService(IGenericRepository<Trainer> trainerRepository,
                              IGenericRepository<Session> sessionRepository)
        {
            _trainerRepository = trainerRepository;
            _sessionRepository = sessionRepository;
        }
        public async Task<bool> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default)
        {
            var emailExist = await _trainerRepository.AnyAsync(t => t.Email == model.Email, ct);
            var phoneExist = await _trainerRepository.AnyAsync(t => t.Phone == model.Phone, ct);

            if (emailExist || phoneExist) return false;

            var trainer = new Trainer()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = new Address()
                {
                    BuildingNumber = model.BuildingNumber,
                    City = model.City,
                    Street = model.Street
                },
                Specialty = model.Specialty
            };

            var result = await _trainerRepository.UpdateAsync(trainer, ct);
            return result > 0;
        }

        public async Task<IEnumerable<TrainerViewModel>> GetAllTrainersAsync(CancellationToken ct = default)
        {
            var trainers = await _trainerRepository.GetAllAsync(ct: ct);
            if (!trainers.Any()) return [];
            return trainers.Select(t => new TrainerViewModel()
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email,
                Phone = t.Phone,
                Specialty = t.Specialty.ToString()
            });

        }

        public async Task<TrainerViewModel?> GetTrainerDetailsByIdAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _trainerRepository.GetByIdAsync(trainerId, ct);
            if (trainer == null) return null;

            var model =  new TrainerViewModel()
            {
                Email = trainer.Email,
                Name = trainer.Name,
                Phone = trainer.Phone,
                Specialty = trainer.Specialty.ToString(),
                DateOfBirth = trainer.DateOfBirth.ToString(),
                Address = $"{trainer.Address.BuildingNumber} - {trainer.Address.Street} - {trainer.Address.City}"
            };

            return model;

        }

        public async Task<TrainerToUpdateViewModel?> GetTrainerToUpdateAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _trainerRepository.GetByIdAsync(trainerId, ct);
            if (trainer == null) return null;

            var model = new TrainerToUpdateViewModel()
            {
                Name = trainer.Name,
                Email = trainer.Email,
                Phone = trainer.Phone,
                BuildingNumber = trainer.Address.BuildingNumber,
                City = trainer.Address.City,
                Street = trainer.Address.Street,
                Specialty = trainer.Specialty
            };
            return model;
        }

        public async Task<bool> RemoveTrainerAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _trainerRepository.GetByIdAsync(trainerId, ct);
            if (trainer == null) return false;

            var hasFutureSessions = await _sessionRepository.AnyAsync(s => s.TrainerId == trainerId && s.StartDate > DateTime.Now);
            if (hasFutureSessions) return false;

            var result = await _trainerRepository.DeleteAsync(trainer, ct);

            return result > 0;
        }

        public async Task<bool> UpdateTrainerDetailsAsync(int id, TrainerToUpdateViewModel model, CancellationToken ct = default)
        {
            var trainer = await _trainerRepository.GetByIdAsync(id, ct);
            if (trainer == null) return false;

            var emailExist = await _trainerRepository.AnyAsync(t => t.Email == trainer.Email && t.Id != id, ct);
            var phoneExist = await _trainerRepository.AnyAsync(t => t.Phone == trainer.Phone && t.Id != id, ct);

            if (emailExist || phoneExist) return false;

            trainer.Email = model.Email;
            trainer.Phone = model.Phone;
            trainer.Address = new Address()
            {
                BuildingNumber = model.BuildingNumber,
                Street = model.Street,
                City = model.City
            };
            trainer.Specialty = model.Specialty;
            trainer.UpdatedAt = DateTime.Now;

            var result = await _trainerRepository.UpdateAsync(trainer, ct);
            return result > 0;
        }
    }
}
