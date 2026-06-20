using AutoMapper;
using AutoMapper.Execution;
using GymManagement.BLL.Common;
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

        public async Task<Result> CreateSessionAsync(CreateSessionViewModel model, CancellationToken ct = default)
        {
            if (model.EndDate <= model.StartDate) return Result.Validation("EndDate Must Be After StartDate");
            if (model.StartDate <= DateTime.Now) return Result.Validation("StartDate Must Be In The Future");
            if (model.Capacity < 1 || model.Capacity > 25) return Result.Validation("Capacity Must Be Between 1 and 25");

            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(model.TrainerId);
            if (trainer is null) return Result.NotFound("Trainer Not Found");

            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(model.CategoryId);
            if (category is null) return Result.NotFound("Category Not Found");

            var isValid = Enum.TryParse<Specialty>(category.CategoryName, true, out var CategorySpecialty);
            if (!isValid || trainer.Specialty != CategorySpecialty) return Result.Validation("Can Not Create This Session To This Trainer");

            var session = _mapper.Map<CreateSessionViewModel, Session>(model);

            _unitOfWork.GetRepository<Session>().Add(session);
            var result = await _unitOfWork.SaveChangesAsync();
            return result > 0 ? Result.OK() : Result.Fail("Failed To Create Session");
        }

        public async Task<Result<IEnumerable<SessionViewModel>>> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var sessionRepo = _unitOfWork.SessionRepository;
            var sessions = await sessionRepo.GetAllSessionsWithTrainerAndCategoryAsync(ct);

            if (sessions == null || !sessions.Any())
                return Result<IEnumerable<SessionViewModel>>.NotFound("No sessions found");

            var mappedSessions = _mapper.Map<IEnumerable<Session>, IEnumerable<SessionViewModel>>(sessions);

            foreach (var session in mappedSessions)
            {
                session.AvailableSlots = session.Capacity - await sessionRepo.GetCountOfBookedSloatsAsync(session.Id, ct);
            }

            return Result<IEnumerable<SessionViewModel>>.OK(mappedSessions);
        }

        public async Task<Result<IEnumerable<CategorySelectViewModel>>> GetCategoriesForDropDownAsync(CancellationToken ct = default)
        {
            var Categories = await _unitOfWork.GetRepository<Category>().GetAllAsync(ct: ct);
            var mappedCategories = _mapper.Map<IEnumerable<Category>, IEnumerable<CategorySelectViewModel>>(Categories);

            return Result<IEnumerable<CategorySelectViewModel>>.OK(mappedCategories);
        }

        public async Task<Result<SessionViewModel>> GetSessionDetailsByIdAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetSessionByIdWithTrainerAndCategory(sessionId, ct);
            if (session == null)
                return Result<SessionViewModel>.NotFound("Session Not Found");

            var mappedSession = _mapper.Map<Session, SessionViewModel>(session);
            mappedSession.AvailableSlots = mappedSession.Capacity - await _unitOfWork.SessionRepository.GetCountOfBookedSloatsAsync(sessionId, ct);
            return Result<SessionViewModel>.OK(mappedSession);
        }

        public async Task<Result<IEnumerable<TrainerSelectViewModel>>> GetTrainersForDropDownAsync(CancellationToken ct = default)
        {
            var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(ct: ct);
            var mappedTrainers = _mapper.Map<IEnumerable<Trainer>, IEnumerable<TrainerSelectViewModel>>(trainers);

            return Result<IEnumerable<TrainerSelectViewModel>>.OK(mappedTrainers);
        }

        public async Task<Result<SessionToUpdateViewModel>> GetSessionToUpdateAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(sessionId, ct);
            if (session == null)
                return Result<SessionToUpdateViewModel>.NotFound("Session Not Found");

            if (session.StartDate <= DateTime.Now)
                return Result<SessionToUpdateViewModel>.Fail("Can Not Update Session that Has Already Started");

            var bookedCount = await _unitOfWork.SessionRepository.GetCountOfBookedSloatsAsync(sessionId, ct);
            if(bookedCount > 0)
                return Result<SessionToUpdateViewModel>.Fail("Can Not Update Session that Has Already Bookings");

            var mappedSession = _mapper.Map<Session, SessionToUpdateViewModel>(session);
            return Result<SessionToUpdateViewModel>.OK(mappedSession);
        }

        public async Task<Result> UpdateSessionDetailsAsync(int id, SessionToUpdateViewModel model, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(id, ct);
            if (session == null)
                return Result.NotFound("Session Not Found");

            if (session.StartDate <= DateTime.Now)
                return Result.Fail("Can Not Edit Session That Has Already Started");

            if (model.EndDate <= model.StartDate)
                return Result.Validation("End Date Must Be After StartDate");

            var BookedCount = await _unitOfWork.SessionRepository.GetCountOfBookedSloatsAsync(id);
            if (BookedCount > 0)
                return Result.Fail("Can Not Update Session that Has Already has Bookings");

            if (model.StartDate <= DateTime.Now)
                return Result.Validation("Start Date Must Be In Future");

            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(model.TrainerId);
            if (trainer is null) return Result.NotFound("Trainer Not Found");

            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(session.CategoryId);

            var isValid = Enum.TryParse<Specialty>(category?.CategoryName, true, out var CategorySpecialty);
            if (!isValid || trainer.Specialty != CategorySpecialty) return Result.Validation("Can Not Create This Session To this Trainer");

            _mapper.Map(model, session);
            session.UpdatedAt = DateTime.Now;

            _unitOfWork.SessionRepository.Update(session);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to Update Session");
        }

        public async Task<Result> RemoveSessionAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(sessionId, ct);
            if(session == null)
                return Result.NotFound("Session Not Found");
            if (session.EndDate >= DateTime.Now)
                return Result.Validation("Cannot Delete Session That Has Not Ended Yet");

            var bookedCount = await _unitOfWork.SessionRepository.GetCountOfBookedSloatsAsync(sessionId, ct);
            if (bookedCount > 0)
                return Result.Fail("Cannot Delete That has Bookings");

            _unitOfWork.SessionRepository.Delete(session); //Delete Local
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.OK() : Result.Fail("Failed to remove Session");

        }
    }
}
