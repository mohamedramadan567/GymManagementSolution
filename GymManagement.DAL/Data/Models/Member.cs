using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Data.Models
{
    public class Member : GymUser
    {
        //JoinDate = CreatedAt in BaseEntity
        public string? Photo { get; set; }

        #region Relationships
        public HealthRecord HealthRecord { get; set; } = default!;

        public ICollection<MemberShip> MemberShips { get; set; } = default!;

        public ICollection<Booking> MemberSessions { get; set; } = default!;

        #endregion
    }
}
