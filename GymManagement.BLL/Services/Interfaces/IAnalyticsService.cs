using GymManagement.BLL.Common;
using GymManagement.BLL.ViewModels.AnalyticsViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<Result<AnalyticsViewModel>> GetDataAsync(CancellationToken ct = default);

    }
}
