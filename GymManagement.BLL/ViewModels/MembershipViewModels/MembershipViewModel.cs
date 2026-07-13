using GymManagement.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.ViewModels.MembershipViewModels
{
    public class MembershipViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = default!;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = default!;
    }
}
