using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.Contact)]
    public class ContactController : Controller
    {
        private readonly DShopContext _context;
		private readonly IEmailSender _emailSender;
		private readonly UserManager<AppUserModel> _userManager;
        public ContactController(DShopContext context, IEmailSender emailSender, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;


		}
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
        {
            
            IQueryable<ContactModel> listContact = _context.Contacts.Where(c => c.Status != 0)
                                                        .Include(u => u.User)
                                                        .Include(r => r.Respondent)
                                                        .OrderByDescending(d => d.DateSent);
			int totalOrder = listContact.Count();
			if (pagesSize <= 0)
				pagesSize = 5;
			int countPages = (int)Math.Ceiling((double)totalOrder / 5);

			if (currentPage > countPages)
				currentPage = countPages;
			if (currentPage < 1)
				currentPage = 1;

			var pagingModel = new PagingModel()
			{
				countpages = countPages,
				currentpage = currentPage,
				generateUrl = (pageNumber) => Url.Action("Index", new
				{
					p = pageNumber,
					pagesSize = pagesSize
				})
			};

			var contacts = await listContact.Skip((currentPage - 1) * pagesSize)
						.Take(pagesSize).ToListAsync();

			ViewBag.pagingModel = pagingModel;
			return View(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var contactModel = await _context.Contacts
                .Include(u => u.User)
				.FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
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
                    TempData[DShopConst.TEMPDATA_ERROR] = "The reply message field is required";
                    return RedirectToAction("Reply", "Contact", new { id = IdContact });
                }
                var user = await _userManager.GetUserAsync(this.User);
    
                var contactModel = await _context.Contacts
               .Include(u => u.User)
               .FirstOrDefaultAsync(m => m.Id == IdContact);
				contactModel.ReplyMessage = replyMessage;
                contactModel.DateRespone = DateTime.Now;
                contactModel.RespondentId = user.Id;
                contactModel.Status = 2;
                _context.Update(contactModel);
                await _context.SaveChangesAsync();

                var infoShop = await _context.InformationShops.FirstOrDefaultAsync();
				await _emailSender.SendEmailContact(contactModel, infoShop);
	

                TempData[DShopConst.TEMPDATA_SUCCESS] = "Send gmail contact successful ";
                return RedirectToAction("Index", "Contact");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Send gmail contact fail " + ex.Message;
                return RedirectToAction("Reply", "Contact", new { id = IdContact });
            }
          
        }

		[Authorize(Roles = RoleName.Administrator)]
		public async Task<IActionResult> Remove(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
			var contacModel = await _context.Contacts
	                    .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
			if (contacModel == null)
			{
				return NotFound();
			}

			try
            {
                var contactModel = await _context.Contacts.FindAsync(id);
				contactModel.Status = 0;
				_context.Update(contactModel);
				await _context.SaveChangesAsync();
				TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete contact successful";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Remove contact fail "+ ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
