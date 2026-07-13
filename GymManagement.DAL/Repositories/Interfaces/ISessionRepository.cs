using GymManagement.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Repositories.Interfaces
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
        Task<IEnumerable<Session>> GetAllSessionsWithTrainerAndCategoryAsync(Expression<Func<Session, bool>>? filter = null, CancellationToken ct = default);

        Task<int> GetCountOfBookedSlotsAsync(int sessionId, CancellationToken ct = default);

        Task<Session> GetSessionByIdWithTrainerAndCategory(int sessionId, CancellationToken ct = default);
    }
}
