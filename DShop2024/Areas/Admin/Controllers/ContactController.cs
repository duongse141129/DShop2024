using DShop2024.EnumData;
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
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
	public class ContactController : Controller
    {
        private readonly DShopContext _dataContext;
		private readonly IEmailSender _emailSender;
		private readonly UserManager<AppUserModel> _userManager;
        private readonly string sidebar = "contact";

        public ContactController(DShopContext context, IEmailSender emailSender, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _userManager = userManager;


		}
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
        {
            ViewBag.sidebar = sidebar;
            IQueryable<ContactModel> listContact = _dataContext.Contacts.Where(c => c.Status != 0)
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
            ViewBag.sidebar = sidebar;
            if (id == null)
            {
                return NotFound();
            }

            var contactModel = await _dataContext.Contacts
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
            ViewBag.sidebar = sidebar;
            try
            {
                if (String.IsNullOrEmpty(replyMessage))
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Message can't null";
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
            ViewBag.sidebar = sidebar;
            if (id == null)
            {
                return NotFound();
            }
			var contacModel = await _dataContext.Contacts
	                    .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
			if (contacModel == null)
			{
				return NotFound();
			}

			try
            {
                var contactModel = await _dataContext.Contacts.FindAsync(id);
				contactModel.Status = 0;
				_dataContext.Update(contactModel);
				await _dataContext.SaveChangesAsync();
				TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete contact successful";
                await _dataContext.SaveChangesAsync();
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
