using GymManagement.BLL.Services.Classes;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.SessionViewModels;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace GymManagement.PL.Controllers
{
    public class SessionsController : Controller
    {
        private readonly ISessionService _sessionService;

        public SessionsController(ISessionService sessionService)
        {
            this._sessionService = sessionService;
        }
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _sessionService.GetAllSessionsAsync(ct);
            return View(result.value);
        }

        #region Create Session
        //GET BaseUrl/Sessions/Create
        //Create - Show empty form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropDownListAsync();
            return View();
        }

        //POST BaseUrl/Sessions/Create {Session}
        //Create - Save submitted form
        [HttpPost]
        public async Task<IActionResult> Create(CreateSessionViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDownListAsync();
                return View(model);
            }

            var result = await _sessionService.CreateSessionAsync(model, ct);

            if (result.success)
            {
                TempData["SuccessMessage"] = "Session Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.error;

            await PopulateDropDownListAsync();
            return View(model);
        }

        #endregion

        private async Task PopulateDropDownListAsync()
        {
            var trainersResult = await _sessionService.GetTrainersForDropDownAsync();
            var categoriesResult = await _sessionService.GetCategoriesForDropDownAsync();

            ViewBag.Trainers = new SelectList(trainersResult.value, "Id", "Name");
            ViewBag.Categories = new SelectList(categoriesResult.value, "Id", "CategoryName");
        }
    }

    
}
