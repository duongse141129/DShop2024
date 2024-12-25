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

        public ContactController(DShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;

        }
        public IActionResult Index()
        {
            var contacts = _dataContext.Contacts.ToList();
            return View(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ContactModel contact = await _dataContext.Contacts.FindAsync(Id);

            return View(contact);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ContactModel contact)
        {

            var exitedContact = await _dataContext.Contacts.FindAsync(Id);

            if (ModelState.IsValid)
            {
                

                if (contact.ImageUpload != null)
                {

                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/Logo");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    exitedContact.LogoImg = imageName;

                }
                exitedContact.ShopName = contact.ShopName;
                exitedContact.Description = contact.Description;
                exitedContact.Map = contact.Map;
                exitedContact.Phone = contact.Phone;
                exitedContact.Email = contact.Email;
                
                _dataContext.Update(exitedContact);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Update contact success";
                return RedirectToAction("Index");
            }

            return View(exitedContact);
        }
    }
}
