using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class ContactController : Controller
    {
        private readonly DShopContext _dataContext;
		private readonly IEmailSender _emailSender;
		private readonly UserManager<AppUserModel> _userManager;

		public ContactController(DShopContext context, IEmailSender emailSender, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _userManager = userManager;


		}
        public async Task<IActionResult> Index()
        {
            var contacts = await _dataContext.Contacts.Where(c => c.Status != 0)
                                                        .Include(u => u.User)
                                                        .Include(r => r.Respondent)
                                                        .OrderByDescending(d => d.DateSent)
                                                        .ToListAsync();
            return View(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ContactModel contact = await _dataContext.Contacts.FindAsync(Id);

            return View(contact);

        }

        [HttpGet]
        public async Task<IActionResult> Reply(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactModel = await _dataContext.Contacts
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contactModel == null)
            {
                return NotFound();
            }

            return View(contactModel);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int IdContact, string replyMessage)
        {
            try
            {
                if (String.IsNullOrEmpty(replyMessage))
                {
                    TempData["error"] = "Message can't null";
                    return RedirectToAction("Reply", "Contact", new { id = IdContact });
                }
                var user = await _userManager.GetUserAsync(this.User);
    
                var contactModel = await _dataContext.Contacts
               .Include(u => u.User)
               .FirstOrDefaultAsync(m => m.Id == IdContact);
				contactModel.ReplyMessage = replyMessage;
                contactModel.DateRespone = DateTime.Now;
                contactModel.RespondentId = user.Id;
                _dataContext.Update(contactModel);
                await _dataContext.SaveChangesAsync();
				//await _emailSender.SendEmailAsync(contactModel.User.Email, contactModel.Subject, replyMessage);

                var infoShop = await _dataContext.InformationShops.FirstOrDefaultAsync();
				await _emailSender.SendEmailContact(contactModel, infoShop);
	

                TempData["success"] = "Send gmail contact successful ";
                return RedirectToAction("Index", "Contact");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Send gmail contact fail " + ex.Message;
                return RedirectToAction("Reply", "Contact", new { id = IdContact });
            }
          
        }




        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var contactModel = await _dataContext.Contacts.FindAsync(id);
                if (contactModel != null)
                {
                    _dataContext.Contacts.Remove(contactModel);
                }
                TempData["success"] = "Delete contact successful";
                await _dataContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["success"] = "Remove contact fail "+ ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
