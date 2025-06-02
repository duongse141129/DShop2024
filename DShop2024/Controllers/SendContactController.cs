using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
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
        public async Task<IActionResult> Send(string subject, string message)
        {
            if(string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
            {
                return Ok(new { success = false, noti = "Please fill all inputs " });
            }
                try
                {
                    ContactModel contactModel = new ContactModel();
                    var user = await _userManager.GetUserAsync(this.User);
                    contactModel.UserId = user.Id;
                    contactModel.DateSent = DateTime.Now;
                    contactModel.Status = 1;
                    contactModel.Subject = subject;
                    contactModel.Message = message;
                    _context.Add(contactModel);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, noti = "Send contact successful "});
                }
                catch (Exception ex)
                {
                    return Ok(new { success = false, noti = "Send contact fail " + ex.Message });
                }

        }


    }
}
