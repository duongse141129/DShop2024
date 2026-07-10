using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.Promotion, SubMenu.Promotion.Coupon)]
    public class CouponController : Controller
	{
		private readonly DShopContext _context;
        private readonly IMapper _mapper;


        public CouponController(DShopContext context, IMapper mapper)
		{
			_context = context;
            _mapper = mapper;
        }
		public async Task<IActionResult> Index(string search = "", [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
		{
            await ChangeValidCoupon();
            IQueryable <CouponModel> listCoupon = _context.Coupons
                                .Where(c => c.Status != 0)
                                .Include(c => c.Promotion)
                                .Where( p => p.Promotion.CategoryCouponName != DShopConst.NEW_CUSTOMER)
                                .OrderByDescending(c => c.Id);
            if (!String.IsNullOrEmpty(search))
            {
                var detailCoupon = await listCoupon.Where(c => c.CouponCode == search).FirstOrDefaultAsync();
                if (detailCoupon!= null)
                    return RedirectToAction("Detail", new {id =detailCoupon.Id });
                listCoupon = listCoupon.Where(c => c.CouponName.Contains(search) || c.Description.Contains(search));
            }
            ViewBag.search = search;
            int totalCoupon = await listCoupon.CountAsync();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalCoupon / 10);

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
                    pagesSize = pagesSize,
                    search = search
                })
            };

            var coupons = await listCoupon.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize)
                        .Include(c => c.OrderCoupons)
                        .ToListAsync();
            var couponViewModels = _mapper.Map<List<CouponViewModel>>(coupons);
            ViewBag.pagingModel = pagingModel;
            ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0), "Id", "CategoryCouponName");
            return View(couponViewModels.OrderByDescending(p => p.Id));
		}


        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet]
		public IActionResult Create()
		{
            
            ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0 && b.CategoryCouponName != DShopConst.NEW_CUSTOMER), "Id", "CategoryCouponName");
			return View();
		}


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( CouponModel couponModel)
        {
            
            ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0 && b.CategoryCouponName != DShopConst.NEW_CUSTOMER), "Id", "CategoryCouponName");

			if (ModelState.IsValid)
            {
				try
				{
                    var couponCodeExit = await _context.Coupons.Where(c => c.CouponCode == couponModel.CouponCode).AnyAsync();
                    if (couponCodeExit)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Coupon Code already exists";
                        return View();
                    }

                    var promotion = await _context.Promotions.FindAsync(couponModel.PromotionId);

                    if(couponModel.DateExpired < couponModel.DateStart)
                    {
                        ModelState.AddModelError("Date expired must >= date start");
                        return View();
                    } 
                    if(couponModel.DateStart < DateTime.Now.Date)
                    {
                        ModelState.AddModelError("Cannot choose date in the past");
                        return View();
                    }

                    if(promotion.CategoryCouponName.Equals(DShopConst.SUB_SUMTOTAL_DISCOUNT))
                    { 
                        if(couponModel.Value < 1000)
                        {
                            ModelState.AddModelError("", $"{promotion.CategoryCouponName} must >= 1000");
                            return View();
                        }
                    }   
                    if(promotion.CategoryCouponName.Equals(DShopConst.PERCENTAGE_DISCOUNT))
                    {
                        if(couponModel.Value >100 || couponModel.Value <1)
                        {
                            ModelState.AddModelError("", $"{promotion.CategoryCouponName} must from 1% to 100%");
                            return View();
                        }
					}
                    couponModel.CouponCode = couponModel.CouponCode.ToUpper();
                    couponModel.Status = 1;
                    _context.Add(couponModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Add coupon successful";
                    return RedirectToAction(nameof(Index));
                }
				catch (Exception ex)
				{
                    TempData[DShopConst.TEMPDATA_ERROR] = "Add coupon fail "+ ex.Message;
                    return View();
                }
            }
            TempData[DShopConst.TEMPDATA_ERROR] = "Please fill out all fields to create";
            return View();
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            
            if (id == null)
			{
				return NotFound();
			}
			var couponModel = await _context.Coupons
				.FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
			if (couponModel == null)
			{
				return NotFound();
			}

            try
            {            
                couponModel.Status = 0;
                _context.Coupons.Update(couponModel);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete coupon successful " ;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Delete coupon fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }           
        }


        public async Task<IActionResult> Detail(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            var couponModel = await _context.Coupons.Include(c => c.Promotion).Include(c => c.OrderCoupons)
                                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (couponModel == null)
            {
                return NotFound();
            }
            var couponViewModel = _mapper.Map<CouponViewModel>(couponModel);
            return View(couponViewModel);
        }

        private async Task ChangeValidCoupon()
        {
            var coupons = await _context.Coupons
                                 .Where(c => c.Status == 2)
                                 .Include(c => c.Promotion)
                                 .Where(p => p.Promotion.CategoryCouponName != DShopConst.NEW_CUSTOMER)
                                 .ToListAsync();
            var today = DateTime.Today;
            bool hasChanges = false;
            foreach (var coupon in coupons)
            {
                bool isInvalid =
                    coupon.Quantity <= 0 ||
                    coupon.DateStart.Date > today ||
                    coupon.DateExpired.Date < today;

                if (isInvalid)
                {
                    coupon.Status = 1;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> ShowCoupon(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            await ChangeValidCoupon();
            var couponModel = await _context.Coupons
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (couponModel == null)
            {
                return NotFound();
            }
            var today = DateTime.Today;
            try
            {
                var countShowCoupon = await _context.Coupons.Where(c => c.Status == 2).CountAsync();
                if(countShowCoupon >= DShopConst.MAX_COUPON_SHOW)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = $"Display is limited to {DShopConst.MAX_COUPON_SHOW} coupons.";
                    return RedirectToAction(nameof(Index));
                }
                if(couponModel.Quantity == 0)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Show coupon fail. Out of coupon";
                    return RedirectToAction(nameof(Index));
                }
                if (couponModel.DateStart.Date > today)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Show coupon fail. The coupon is not ready yet.";
                    return RedirectToAction(nameof(Index));
                }
                if (couponModel.DateExpired.Date < today)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Show coupon fail. Coupon was expired";
                    return RedirectToAction(nameof(Index));
                }

                couponModel.Status = 2;
                _context.Coupons.Update(couponModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Show coupon fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> HideCoupon(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            await ChangeValidCoupon();
            var couponModel = await _context.Coupons
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (couponModel == null)
            {
                return NotFound();
            }
            try
            {
                couponModel.Status = 1;
                _context.Coupons.Update(couponModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Hide coupon fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
