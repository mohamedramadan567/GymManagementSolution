using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Common
{
    public enum ResultKind
    {
        OK,
        NotFound, 
        Conflict,
        ValidationFailed,
        Forbidden
    }
}
