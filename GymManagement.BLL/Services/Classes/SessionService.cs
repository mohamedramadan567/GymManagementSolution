using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.SessionViewModels;
using GymManagement.DAL.Data.Models;
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

        public SessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<SessionViewModel>?> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var sessionRepo = _unitOfWork.SessionRepository;
            var sessions = await sessionRepo.GetAllSessionsWithTrainerAndCategoryAsync(ct);
            if (sessions == null || !sessions.Any()) return null;

            var mappedSessions = sessions.Select(s => new SessionViewModel
            {
                Id = s.Id,
                Capacity = s.Capacity,
                Description = s.Description,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                CategoryName = s.Category.CategoryName,
                TrainerName = s.Trianer.Name,
            });

            foreach (var session in mappedSessions)
            {
                session.AvailableSlots = session.Capacity -  await sessionRepo.GetCountOfBookedSloatsAsync(session.Id, ct);
            }

            return mappedSessions;
        }
    }
}
