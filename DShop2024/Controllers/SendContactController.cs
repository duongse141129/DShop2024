using DShop2024.EnumData;
using DShop2024.Hubs;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    public class SendContactController : Controller
	{
		private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public SendContactController(DShopContext context, UserManager<AppUserModel> userManager, IHubContext<ChatHub> hubContext)
		{
			_context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }
		public async Task<IActionResult> Index()
		{
            var faqs = await _context.FAQs.ToListAsync();
            ViewBag.Subjects = new SelectList(ContactEnumData.typeSubject.ToList());
            ViewBag.FAQs = faqs;    
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
                    ContactViewModel contactViewModel = new ContactViewModel
                    {
                        ContactId = contactModel.Id,
                        UserName = user.UserName,
                        Avatar = user.Avatar,
                        DateSent = contactModel.DateSent.ToString("MM/dd/yyyy h:mm tt"),
                        Subject = contactModel.Subject,
                        PathImage = $" {DShopConst.SEVER_ADDRESS}/media/avatar/{user.Avatar}",
                        LinkContact = $" {DShopConst.SEVER_ADDRESS}/Admin/Contact/Reply/{contactModel.Id}"
                    };
                    await _hubContext.Clients.All.SendAsync("SendContact", user.UserName, contactViewModel);
                    return Ok(new { success = true, noti = "Send contact successful "});
                }
                catch (Exception ex)
                {
                    return Ok(new { success = false, noti = "Send contact fail " + ex.Message });
                }

        }


    }
}
