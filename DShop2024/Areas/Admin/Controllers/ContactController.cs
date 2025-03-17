using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        public ContactController(DShopContext context, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;

        }
        public async Task<IActionResult> Index()
        {
            var contacts = await _dataContext.Contacts.Where(c => c.Status != 0)
                                                        .Include(u => u.User).OrderBy(d => d.DateSent)
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

                var contactModel = await _dataContext.Contacts
               .Include(u => u.User)
               .FirstOrDefaultAsync(m => m.Id == IdContact);
                await _emailSender.SendEmailAsync(contactModel.User.Email, contactModel.Subject, replyMessage);

                TempData["success"] = "Send gmail contact successful ";
                return RedirectToAction("Reply", "Contact", new { id = IdContact });
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
