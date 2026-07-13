using GymManagement.BLL.Services.Classes;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.BLL.ViewModels.SessionViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace GymManagement.PL.Controllers
{
    [Authorize]
    public class MembershipsController : Controller
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var memberships = await _membershipService.GetAllMembershipsAsync(ct);
            return View(memberships);
        }

        #region Create Membership
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateDropDownListAsync(ct);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMembershipViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDownListAsync(ct);
                return View(model);
            }

            var result = await _membershipService.CreateMembershipAsync(model, ct);

            if (result.success)
            {
                TempData["SuccessMessage"] = "Membership Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.error;

            await PopulateDropDownListAsync(ct);
            return View(model);
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> Cancel (int id, CancellationToken ct)
        {
            var result = await _membershipService.DeleteActiveMembershipAsync(id, ct);

            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] = result.success ? "Membership Cancelled Successfully" : result.error;
            return RedirectToAction(nameof(Index));

        }

        private async Task PopulateDropDownListAsync(CancellationToken ct = default)
        {
            var membersResult = await _membershipService.GetMembersForDropDownAsync(ct);
            var plansResult = await _membershipService.GetPlansForDropDownAsync(ct);

            ViewBag.Members = new SelectList(membersResult, "Id", "Name");
            ViewBag.Plans = new SelectList(plansResult, "Id", "Name");
        }
    }
}
