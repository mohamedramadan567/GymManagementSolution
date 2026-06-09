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
    public class PlanRepository : GenericRepository<Plan>, IPlanRepository
    {
        private readonly GymDbContext dbContext;
        public PlanRepository(GymDbContext dbContext): base(dbContext)
        {
        }

        public Task<IEnumerable<Plan>> GetPlansWithMembers()
        {
            throw new NotImplementedException();
        }
    }
}
