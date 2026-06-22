using GymManagement.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Repositories.Interfaces
{
    public interface IMembershipRepository : IGenericRepository<MemberShip>
    {
        Task<IEnumerable<MemberShip>> GetAllMembershipsWithMemberAndPlanAsync(Expression<Func<MemberShip, bool>>? filter = null, CancellationToken ct = default);
    }
}
