using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.AnalyticsViewModels;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GymManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IAnalyticsService analyticsService, ILogger<HomeController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var data = await _analyticsService.GetDataAsync(ct);
            if(!data.success)
                return View(Enumerable.Empty<AnalyticsViewModel>());

            return View(data.value);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
