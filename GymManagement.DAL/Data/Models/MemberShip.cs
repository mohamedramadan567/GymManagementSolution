using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Data.Models
{
    public class MemberShip : BaseEntity
    {
        public Member Member { get; set; } = default!;
        public int MemberId { get; set; }
        public Plan Plan { get; set; } = default!;
        public int PlanId { get; set; }

        //StartDate = CreatedAt
        public DateTime EndDate { get; set; }


        //NotMapped
        public string Status => EndDate > DateTime.Now ? "Active" : "Expired";
        public bool IsActive => EndDate > DateTime.Now;
    }
}
