using GymManagement.DAL.Repositories.Classes;
using GymManagement.DAL.Repositories.Interfaces;
using GymManagement.DAL.Data.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GymManagement.Controllers
{
    public class PlansController : Controller
    {
        //private readonly GymDbContext dbContext;
        private readonly IPlanRepository planRepository;
        public PlansController(IPlanRepository planRepository)
        {
            this.planRepository = planRepository;
        }

        //GET BaseUrl/Plans/Index -> listing All Plans
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var plans = await planRepository.GetAllAsync(ct: ct);
            return View(plans);
        }

        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var plan = await planRepository.GetById(id, ct);

            if(plan is null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(plan);
            }
        }
    }
}
