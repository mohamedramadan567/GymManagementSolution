using GymManagement.DAL.Data.DbContexts;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Repositories.Classes
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        private readonly GymDbContext _dbContext;

        public SessionRepository(GymDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Session>> GetAllSessionsWithTrainerAndCategoryAsync(CancellationToken ct = default)
        {
            var query = _dbContext.Sessions.AsNoTracking().Include(s => s.Trianer).Include(s => s.Category);
            return await query.ToListAsync();

        }

        public async Task<int> GetCountOfBookedSloatsAsync(int sessionId, CancellationToken ct = default) 
            => await _dbContext.Bookings.AsNoTracking().CountAsync(b => b.SessionId == sessionId);

        public async Task<Session?> GetSessionByIdWithTrainerAndCategory(int sessionId, CancellationToken ct = default)
        {
            return await _dbContext.Sessions.AsNoTracking().Include(s => s.Trianer).Include(s => s.Category).FirstOrDefaultAsync(x => x.Id == sessionId, ct);
        }
    }
}
