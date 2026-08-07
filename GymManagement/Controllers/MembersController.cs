using GymManagement.BLL.Services.Attachment;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace GymManagement.PL.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class MembersController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly IAttachmentService _attachmentService;

        public MembersController(IMemberService memberService, IAttachmentService attachmentService)
        {
            _memberService = memberService;
            _attachmentService = attachmentService;
        }


        #region Get Member Photo
        [HttpGet]
        public async Task<IActionResult> Picture(int id)
        {
            var member = await _memberService.GetMemberDetailsByIdAsync(id);
            if (member == null || string.IsNullOrWhiteSpace(member.value.Photo))
                return NotFound();

            var result = _attachmentService.GetFile(member.value.Photo, "MembersPhoto");
            if (!result.success) return NotFound();

            return File(result.value.stream, result.value.contentType);
        }
        #endregion

        //GET BaseUrl/Members/Index
        //Index - List all members (supports optional search)
        public async Task<IActionResult> Index(string? search, CancellationToken ct)
        {
            var members = await _memberService.GetAllMembersAsync(search, ct);
            if(!members.success)
            {
                return View(Enumerable.Empty<MemberViewModel>());
            }
            ViewBag.Search = search;
            return View(members.value);
        }

        //GET BaseUrl/Members/MemberDetails/{id}  //optional id
        //MemberDetails - Show one member's details
        public async Task<IActionResult> MemberDetails(int id, CancellationToken ct)
        {
            var member = await _memberService.GetMemberDetailsByIdAsync(id, ct);

            if(!member.success)
            {
                TempData["ErrorMessage"] =  member.error ?? "Member Not Found";
                return RedirectToAction(nameof(Index));
            }

            return View(member.value);
        }

        //GET BaseUrl/Members/HealthRecordDetails/{id}  //optional id
        //HealthRecordDetails - Show one member's details

        public async Task<IActionResult> HealthRecordDetails(int id, CancellationToken ct)
        {
            //Get Member by id
            var member = await _memberService.GetMemberHealthRecordByIdAsync(id, ct);

            if(!member.success)
            {
                TempData["ErrorMessage"] = member.error ?? "Health Record not Found";
                return RedirectToAction(nameof(Index));
            }

            return View(member.value);
            //Check is member null => return Index with message
            //member is not null => return view data
        }

        #region Create Member
        //GET BaseUrl/Members/Create
        //Create - Show empty form
        [HttpGet]
        public IActionResult Create() => View();

        //POST BaseUrl/Members/Create {Member}
        //CreateMember - Save submitted form
        public async Task<IActionResult> CreateMember(CreateMemberViewModel member, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return View(nameof(Create), member);

            var result = await _memberService.CreateMemberAsync(member, ct);

            if (result.success)
                TempData["SuccessMessage"] = "Member Created Successfully";
            else
                TempData["ErrorMessage"] = result.error;

                return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Edit Member
        //GET BaseUrl/Members/Edit/{id}
        //Edit - Show form pre-filled
        [HttpGet]
        public async Task<IActionResult> EditMember(int id, CancellationToken ct)
        {
            var member = await _memberService.GetMemberToUpdateAsync(id, ct);
            
            if(!member.success)
            {
                TempData["ErrorMessage"] = member.error ?? "Member Not Found";
                return RedirectToAction(nameof(Index));
            }

            return View(member.value);
        }

        //POST BaseUrl/Members/Edit {Member}
        //Edit - Save edits

        public async Task<IActionResult> EditMember([FromRoute]int id, MemberToUpdateViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _memberService.UpdateMemberDetailsAsync(id, model, ct);

            if (result.success)
                TempData["SuccessMessage"] = "Member Updated Successfully";
            else
                TempData["ErrorMessage"] = result.error;

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Delete Member
        //GET BaseUrl/Members/Delete/{id}
        //Delete - Shows deletion confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var member = await _memberService.GetMemberDetailsByIdAsync(id, ct);

            if(!member.success)
            {
                TempData["ErrorMessage"] = member.error ?? "Member Not Found";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        //POST BaseUrl/Members/DeleteConfirmed/{id}
        //DeleteConfirmed - Processes deletion 

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed([FromRoute]int id, CancellationToken ct)
        {
            var result = await _memberService.RemoveMemberAsync(id, ct);

            if (result.success)
                TempData["SuccessMessage"] = "Member Deleted Successfully";
            else
                TempData["ErrorMessage"] = result.error;

            return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
