using GymManagement.DAL.Repositories.Classes;
using GymManagement.DAL.Repositories.Interfaces;
using GymManagement.DAL.Data.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GymManagement.DAL.Data.Models;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.PlanViewModels;

namespace GymManagement.Controllers
{
    public class PlansController : Controller
    {
        //private readonly GymDbContext dbContext;
        //private readonly IGenericRepository<Plan> _planRepository;
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            //_planRepository = planRepository;
            _planService = planService;
        }

        //GET BaseUrl/Plans/Index -> listing All Plans
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var plans = await _planService.GetAllPlansAsync(ct);
            return View(plans);
        }

        //GET BaseUrl/Plans/Index
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var plan = await _planService.GetPlanByIdAsync(id, ct);

            if (plan is null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(plan);
            }
        }

        #region Edit
        //GET BaseUrl/Plans/Edit/{id}
        //Edit - Show form pre-filled
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var plan = await _planService.GetPlanToUpdateAsync(id, ct);
            if(plan is null)
            {
                TempData["ErrorMessage"] = "Plan cannot be edited (not found, inactive, or has active memberships). ";
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }


        //POST BaseUrl/Plans/Edit {Plan}
        //Edit - Save edits
        [HttpPost]
        public async Task<IActionResult> Edit(int id,  PlanToUpdateViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _planService.UpdatePlanDetailsAsync(id, model, ct);
            if(result)
            {
                TempData["SuccessMessage"] = "Plan Updated Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Faild to Update Plan";
            }
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _planService.ToggleActivationAsync(id, ct);
            if (result)
                TempData["SuccessMessage"] = "Plan Status Changed";
            else
                TempData["ErrorMessage"] = "Failed to Toggle Plan Status";
            return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
