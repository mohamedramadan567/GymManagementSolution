using GymManagement.DAL.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Data.Models
{
    public class Trainer : GymUser
    {
        public Specialty Specialty { get; set; }

        //HireDate = CreatedAt in BaseEntity

        public ICollection<Session> Sessions { get; set; } = default!;

    }
}
