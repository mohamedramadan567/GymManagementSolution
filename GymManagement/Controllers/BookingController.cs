using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.BookingViewModels;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.PL.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var sessions = await _bookingService.GetAllSessionsAsync(ct);
            return View(sessions);
        }

        [HttpGet]
        public async Task<IActionResult> GetMembersForUpcomingSession(int Id, CancellationToken ct)
        {
            var members = await _bookingService.GetMembersForSession(sessionId: Id, ct);
            return View(members);
        }


        [HttpGet]
        public async Task<IActionResult> GetMembersForOngoingSessions(int Id, CancellationToken ct)
        {
            var members = await _bookingService.GetMembersForSession(sessionId: Id, ct);
            return View(members);
        }


        [HttpGet]
        public async Task<IActionResult> Create(int Id, CancellationToken ct)
        {
            var members = await GetMemberForDropDown(Id, ct);
            ViewBag.Members = new SelectList(members, "Id", "Name");
            ViewBag.Session = Id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingViewModel model, CancellationToken cancellationToken)
        {
            var result = await _bookingService.CreateBooking(model, cancellationToken);

            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] =
                result.success ? "Booking created successfully." : result.error;

            return RedirectToAction(nameof(GetMembersForUpcomingSession), new { id = model.SessionId });
        }

        [HttpPost]
        public async Task<IActionResult> Attended(int memberId, int sessionId, CancellationToken cancellationToken)
        {
            var result = await _bookingService.MarkAttendedAsync(memberId, sessionId, cancellationToken);

            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] =
                result.success ? "Marked attended successfully." : result.error;

            return RedirectToAction(actionName: nameof(GetMembersForOngoingSessions), routeValues: new { id = sessionId });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int memberId, int sessionId, CancellationToken cancellationToken)
        {
            var result = await _bookingService.CancelBookingAsync(memberId, sessionId, cancellationToken);

            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] =
                result.success ? "Booking canceled successfully." : result.error;

            return RedirectToAction(actionName: nameof(GetMembersForUpcomingSession), routeValues: new { id = sessionId });
        }

        private async Task<IEnumerable<MemberSelectViewModel>> GetMemberForDropDown(int sessionId, CancellationToken ct)
        {
            var members = await _bookingService.GetMembersForDropDown(sessionId, ct);
            return members;
        }
    }
}
