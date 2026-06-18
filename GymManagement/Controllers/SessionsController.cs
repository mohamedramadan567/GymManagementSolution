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
            var sessions = await _sessionService.GetAllSessionsAsync(ct);
            return View(sessions);
        }

        #region Create Trainer
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

            if (result)
            {
                TempData["SuccessMessage"] = "Session Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Faild to Create Session";

            await PopulateDropDownListAsync();
            return View(model);
        }

        #endregion

        private async Task PopulateDropDownListAsync()
        {
            //to sent drop down list to view
            ViewBag.Trainers = new SelectList(await _sessionService.GetTrainersForDropDownAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _sessionService.GetCategoriesForDropDownAsync(), "Id", "CategoryName");
        }
    }

    
}
