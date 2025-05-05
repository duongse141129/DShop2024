using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
		public async Task<IActionResult> Index()
		{
            var FAQs = await _context.FAQs.ToListAsync();
            ViewBag.FAQs = FAQs;
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
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Send contact successful";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Send contact fail "+ ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction("Index");
        }


    }
}
