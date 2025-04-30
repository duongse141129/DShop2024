using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
	[Authorize]
	public class SendContactController : Controller
	{
		private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;

        public SendContactController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_context = context;
            _userManager = userManager;
        }
		public IActionResult Index()
		{
			return View();
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([Bind("Subject,Message")] ContactModel contactModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(this.User);
                    contactModel.UserId = user.Id;
                    contactModel.DateSent = DateTime.Now;
                    contactModel.Status = 1;
                    _context.Add(contactModel);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Send contact successful";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Send contact fail "+ ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction("Index");
        }


    }
}
