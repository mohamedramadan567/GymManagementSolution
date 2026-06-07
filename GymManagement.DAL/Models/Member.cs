using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Models
{
    public class Member : GymUser
    {
        //JoinDate = CreatedAt in BaseEntity
        public string? Photo { get; set; }
    }
}
