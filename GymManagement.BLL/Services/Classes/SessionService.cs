using AutoMapper;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.SessionViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Data.Models.Enums;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SessionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            this._mapper = mapper;
        }

        public async Task<bool> CreateSessionAsync(CreateSessionViewModel model, CancellationToken ct = default)
        {
            if (model.EndDate <= model.StartDate) return false;
            if (model.StartDate <= DateTime.Now) return false;
            if (model.Capacity < 1 || model.Capacity > 25) return false;

            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(model.TrainerId);
            if (trainer is null) return false;

            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(model.CategoryId);
            if (category is null) return false;

            var isValid = Enum.TryParse<Specialty>(category.CategoryName, true, out var CategorySpecialty);
            if (!isValid || trainer.Specialty != CategorySpecialty) return false;

            var session = _mapper.Map<CreateSessionViewModel, Session>(model);

            _unitOfWork.GetRepository<Session>().Add(session);
            var result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<SessionViewModel>?> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var sessionRepo = _unitOfWork.SessionRepository;
            var sessions = await sessionRepo.GetAllSessionsWithTrainerAndCategoryAsync(ct);
            if (sessions == null || !sessions.Any()) return null;

            var mappedSessions = _mapper.Map<IEnumerable<Session>, IEnumerable<SessionViewModel>>(sessions);

            foreach (var session in mappedSessions)
            {
                session.AvailableSlots = session.Capacity -  await sessionRepo.GetCountOfBookedSloatsAsync(session.Id, ct);
            }

            return mappedSessions;
        }

        public async Task<IEnumerable<CategorySelectViewModel>> GetCategoriesForDropDownAsync(CancellationToken ct = default)
        {
            var Categories = await _unitOfWork.GetRepository<Category>().GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<Category>, IEnumerable<CategorySelectViewModel>>(Categories);
        }

        public async Task<IEnumerable<TrainerSelectViewModel>> GetTrainersForDropDownAsync(CancellationToken ct = default)
        {
            var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<Trainer>, IEnumerable<TrainerSelectViewModel>>(trainers);
        }
    }
}
