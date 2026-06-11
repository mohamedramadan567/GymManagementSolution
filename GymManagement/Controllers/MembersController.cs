using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace GymManagement.PL.Controllers
{
    public class MembersController : Controller
    {
        private readonly IMemberService _memberService;

        //GET BaseUrl/Members/Index
        //Index - List all members
        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var members = await _memberService.GetAllMembersAsync(ct);
            return View(members);
        }

        //GET BaseUrl/Members/Details/{id}  //optional id
        //Details - Show one member's details

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

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Edit Member
        //GET BaseUrl/Members/Edit/{id}
        //Edit - Show form pre-filled

        //POST BaseUrl/Members/Edit {Member}
        //Edit - Save edits
        #endregion

        #region Delete Member
        //GET BaseUrl/Members/Delete/{id}
        //Delete - Shows deletion confirmation page

        //POST BaseUrl/Members/DeleteConfirmed/{id}
        //DeleteConfirmed - Processes deletion 
        #endregion
    }
}
