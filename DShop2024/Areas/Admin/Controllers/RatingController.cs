using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.CustomerService, SubMenu.CustomerService.Feedback)]
    public class RatingController : Controller
    {

        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;

        public RatingController(DShopContext context, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            var ListRating = _context.Ratings.Where(r => r.Status != 0)
                                    .Include(r => r.Product)
                                    .Include(r => r.User)
                                    .Include(r => r.ReplyBy)
                                    .OrderByDescending(r => r.RatingDateTime);
            int totalRating = await ListRating.CountAsync();
            ViewBag.totalOrder = totalRating;
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalRating / 10);

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

            var ratings = await ListRating.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize)
                        .Select(r => new RatingViewModel
                        {
                            Rating = r,
                            ReplyByRole = _context.UserRoles
                            .Where(ur => ur.UserId == r.UserIdReply)
                            .Join(_context.Roles,
                                  ur => ur.RoleId,
                                  role => role.Id,
                                  (ur, role) => role.Name)
                            .FirstOrDefault()
                        })
                        .ToListAsync();

            ViewBag.pagingModel = pagingModel;
            return View(ratings);
        }

        [HttpGet]
        public async Task<IActionResult> GetPopUpReplyRating(int? Id)
        {

            if (Id == null)
            {
                return NotFound();
            }
            var ratingModel = await _context.Ratings.Include(p => p.User).Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (ratingModel == null)
            {
                return NotFound();
            }
            return PartialView("_ModalReplyRatingPartial", ratingModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReplyRating(int? Id, string ReplyMessage)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var ratingModel = await _context.Ratings
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (ratingModel == null)
            {
                return NotFound();
            }
            if (String.IsNullOrEmpty(ReplyMessage))
            {
                return Ok(new { success = false, Message = "Input reply message " });
            }
            try
            {
                var user = await _userManager.GetUserAsync(this.User);
                ratingModel.ReplyMessage = ReplyMessage;
                ratingModel.UserIdReply = user.Id;
                _context.Ratings.Update(ratingModel);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, Message = "Reply rating successful" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = "Reply rating fail " + ex.Message });
            }
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet]
        public async Task<IActionResult> RemoveRatingPopup(int? Id)
        {

            if (Id == null)
            {
                return NotFound();
            }
            var ratingModel = await _context.Ratings.Include(p => p.User).Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (ratingModel == null)
            {
                return NotFound();
            }
            return PartialView("_ModalRemoveRatingPartial", ratingModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        public async Task<IActionResult> RemoveRating(int? Id)
        {

            if (Id == null)
            {
                return NotFound();
            }
            var ratingModel = await _context.Ratings
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (ratingModel == null)
            {
                return NotFound();
            }
            try
            {
                var user = await _userManager.GetUserAsync(this.User);
                ratingModel.Status = 0;
                _context.Ratings.Update(ratingModel);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, Message = "Remove rating successful" });

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = "Remove rating fail " + ex.Message });
            }
        }
    }
}
