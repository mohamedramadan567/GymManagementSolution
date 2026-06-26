using GymManagement.BLL.Services.Classes;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.BLL.ViewModels.SessionViewModels;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace GymManagement.PL.Controllers
{
    [Authorize]
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
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropDownListAsync();
            return View();
        }

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


        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var session = await _sessionService.GetSessionDetailsByIdAsync(id, ct);

            if (!session.success)
            {
                TempData["ErrorMessage"] = session.error ?? "Session Not Found";
                return RedirectToAction(nameof(Index));
            }

            return View(session.value);
        }


        #region Edit Session
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var session = await _sessionService.GetSessionToUpdateAsync(id, ct);

            if (!session.success)
            {
                TempData["ErrorMessage"] = session.error;
                return RedirectToAction(nameof(Index));
            }
            var trainersResult = await _sessionService.GetTrainersForDropDownAsync();
            ViewBag.Trainers = new SelectList(trainersResult.value, "Id", "Name");

            return View(session.value);
        }


        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute] int id, SessionToUpdateViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var trainersResult01 = await _sessionService.GetTrainersForDropDownAsync();
                ViewBag.Trainers = new SelectList(trainersResult01.value, "Id", "Name");
                return View(model);
            }

            var result = await _sessionService.UpdateSessionDetailsAsync(id, model, ct);

            if (result.success)
            {
                TempData["SuccessMessage"] = "Session Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = result.error;
                var trainersResult = await _sessionService.GetTrainersForDropDownAsync();
                ViewBag.Trainers = new SelectList(trainersResult.value, "Id", "Name");
                return View(model);
            }

            
        }
        #endregion


        #region Delete Session
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var session = await _sessionService.GetSessionDetailsByIdAsync(id, ct);

            if (!session.success)
            {
                TempData["ErrorMessage"] = session.error;
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id, CancellationToken ct)
        {
            var result = await _sessionService.RemoveSessionAsync(id, ct);

            //if (result.success)
            //    TempData["SuccessMessage"] = "Session Deleted Successfully";
            //else
            //    TempData["ErrorMessage"] = result.error;


            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] = result.success ? "Session Deleted Successfully" : result.error;
            return RedirectToAction(nameof(Index));

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
