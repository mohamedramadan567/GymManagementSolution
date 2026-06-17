using GymManagement.BLL.Services.Classes;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymManagement.PL.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ITrainerService _trainerService;

        public TrainersController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }


        //GET BaseUrl/Trainers/Index
        //Index - List all trainers
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var trainers = await _trainerService.GetAllTrainersAsync(ct);
            return View(trainers);
        }


        #region Create Trainer
        //GET BaseUrl/Trainers/Create
        //Create - Show empty form
        [HttpGet]
        public IActionResult Create() => View();

        //POST BaseUrl/Trainers/Create {Trainer}
        //CreateTrainer - Save submitted form
        public async Task<IActionResult> Create(CreateTrainerViewModel trainer, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return View(nameof(Create), trainer);

            var result = await _trainerService.CreateTrainerAsync(trainer, ct);

            if (result)
                TempData["SuccessMessage"] = "Trainer Created Successfully";
            else
                TempData["ErrorMessage"] = "Faild to Create Trainer";

            return RedirectToAction(nameof(Index));
        }

        #endregion

        //GET BaseUrl/Trainers/Details/{id}  //optional id
        //Details - Show one trainer's details
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerDetailsByIdAsync(id, ct);

            if (trainer is null)
            {
                TempData["ErrorMessage"] = "Trainer Not Found";
                return RedirectToAction(nameof(Index));
            }

            return View(trainer);
        }


        #region Edit Member
        //GET BaseUrl/Trainers/Edit/{id}
        //Edit - Show form pre-filled
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerToUpdateAsync(id, ct);

            if (trainer == null)
            {
                TempData["ErrorMessage"] = "Trainer Not Found";
                return RedirectToAction(nameof(Index));
            }

            return View(trainer);
        }

        //POST BaseUrl/Trainers/Edit {Trainer}
        //Edit - Save edits

        public async Task<IActionResult> Edit([FromRoute] int id, TrainerToUpdateViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _trainerService.UpdateTrainerDetailsAsync(id, model, ct);

            if (result)
                TempData["SuccessMessage"] = "Trainer Updated Successfully";
            else
                TempData["ErrorMessage"] = "Faild to Update Trainer";

            return RedirectToAction(nameof(Index));
        }
        #endregion


        #region Delete Trainer
        //GET BaseUrl/Trainers/Delete/{id}
        //Delete - Shows deletion confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerDetailsByIdAsync(id, ct);

            if (trainer == null)
            {
                TempData["ErrorMessage"] = "Trainer Not Found";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        //POST BaseUrl/Trainers/DeleteConfirmed/{id}
        //DeleteConfirmed - Processes deletion 

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id, CancellationToken ct)
        {
            var result = await _trainerService.RemoveTrainerAsync(id, ct);

            if (result)
                TempData["SuccessMessage"] = "Trainer Deleted Successfully";
            else
                TempData["ErrorMessage"] = "Faild to Delete Trainer";

            return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
