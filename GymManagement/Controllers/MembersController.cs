using GymManagement.BLL.Services.Interfaces;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        //POST BaseUrl/Members/Create {Member}
        //CreateMember - Save submitted form
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
