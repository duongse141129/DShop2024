using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = RoleName.Administrator)]
	public class CouponController : Controller
	{
		private readonly DShopContext _context;

		public CouponController(DShopContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index(string search = "", [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
		{
  
            IQueryable <CouponModel> listCoupon = _context.Coupons
                                .Where(c => c.Status != 0)
                                .Include(c => c.Promotion)
                                .OrderByDescending(c => c.Id);
            var count = await listCoupon.CountAsync();
            if (count > 0)
            {
                if (!String.IsNullOrEmpty(search))
                {
                    listCoupon = listCoupon.Where(c => c.CouponName.Contains(search) || c.Description.Contains(search));
                }
            }
            ViewBag.search = search;
            int totalCoupon = listCoupon.Count();
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
                        .Take(pagesSize).ToListAsync();

            ViewBag.pagingModel = pagingModel;
            ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0), "Id", "CategoryCouponName");
            return View(coupons);
		}

		[HttpGet]
		public IActionResult Create()
		{
			ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0 && b.CategoryCouponName != DShopConst.NEW_CUSTOMER), "Id", "CategoryCouponName");
			return View();
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( CouponModel couponModel)
        {
			ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0 && b.CategoryCouponName != DShopConst.NEW_CUSTOMER), "Id", "CategoryCouponName");

			if (ModelState.IsValid)
            {
				try
				{
                    var promotion = await _context.Promotions.FindAsync(couponModel.PromotionId);

                    if(couponModel.DateExpired < couponModel.DateStart)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "DateExpired must >= date start";
                        return RedirectToAction(nameof(Create));
                    } 
                    if(couponModel.DateStart < DateTime.Now.Date)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Cannot choose date in the past";
                        return RedirectToAction(nameof(Create));
                    }

                    if(promotion.CategoryCouponName.Equals(DShopConst.SUB_SUMTOTAL_DISCOUNT))
                    { 
                        if(couponModel.Value < 1000)
                        {
							TempData[DShopConst.TEMPDATA_ERROR] = $"{promotion.CategoryCouponName} must >= 1000";
							return RedirectToAction(nameof(Create));
						}						
					}   
                    if(promotion.CategoryCouponName.Equals(DShopConst.PERCENTAGE_DISCOUNT))
                    {
                        if(couponModel.Value >100 || couponModel.Value <1)
                        {
							TempData[DShopConst.TEMPDATA_ERROR] = $"{promotion.CategoryCouponName} must from 1% to 100%";
							return RedirectToAction(nameof(Create));
						}
					}


                    couponModel.Status = 1;
                    _context.Add(couponModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Add coupon successful";
                    return RedirectToAction(nameof(Index));
                }
				catch (Exception ex)
				{
                    TempData[DShopConst.TEMPDATA_ERROR] = "Add coupon fail "+ ex.Message;
                    return RedirectToAction(nameof(Create));
                }
            }
            TempData[DShopConst.TEMPDATA_ERROR] = "Check all fields";
            return RedirectToAction(nameof(Create));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Not found ";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var couponModel = await _context.Coupons
                .FirstOrDefaultAsync(m => m.Id == id);
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


    }
}
