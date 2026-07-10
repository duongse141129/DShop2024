using AutoMapper;
using DShop2024.Areas.Admin.Models.Sale;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator)]
    [SidebarMenu(Menu.Admin.Promotion, SubMenu.Promotion.Sale)]
    public class SaleController : Controller
    {
        private readonly DShopContext _context;
        private readonly IMapper _mapper;
        public SaleController(DShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ActionResult> IndexAsync(string search = "",[FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            IQueryable<SaleModel> listSaleModel = _context.Sales
                            .Where(s => s.Status != 0)
                            .Include(p => p.Product)
                            .OrderByDescending(o => o.SaleStartDate);
            if (!String.IsNullOrEmpty(search))
            {
                listSaleModel = listSaleModel.Where(c => c.Product.ProductName.Contains(search));
            }
            int totalSale = await listSaleModel.CountAsync();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalSale / 10);

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

            var sales = await listSaleModel.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize).ToListAsync();

            ViewBag.pagingModel = pagingModel;
            ViewBag.search = search;
            var saleVMs = _mapper.Map<List<SaleViewModel>>(sales);
            return View(saleVMs);
        }

        [HttpGet]
        public async Task<IActionResult> SearchLive(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return PartialView("_SearchResultPartial", new List<ProductModel>());
            ViewBag.Keyword = keyword;
            var products = await _context.Products
                .Where(x => x.Status != 0 && x.ProductName.Contains(keyword))
                .OrderBy(x => x.ProductName)
                .Take(5)
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .ToListAsync();
            return PartialView("_SearchResultPartial", products);
        }

        [HttpGet]
        public  IActionResult GetPopUpAddSale()
        {
            return PartialView("_AddProductToSalePartial");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesRequest model)
        {
            if (model.Items == null || !model.Items.Any())
                return Ok(new { success = false, Message = "Must select at least one product." });

            if (model.SaleStartDate < DateTime.Now)
                return Ok(new { success = false, Message = "Start date must be in the future." });

            if (model.SaleStartDate >= model.SaleEndDate)
                return Ok(new { success = false, Message = "Start date must be earlier than End date." });



            try
            {
                var conflictingItems = new List<string>();

                foreach (var item in model.Items)
                {
                    bool hasConflict = _context.Sales.Where(s => s.Status != 0).Any(s =>
                        s.ProductId == item.ProductId &&
                        s.SaleStartDate < model.SaleEndDate &&
                        s.SaleEndDate > model.SaleStartDate);

                    if (hasConflict)
                    {
                        var productName = _context.Products.Find(item.ProductId)?.ProductName;
                        conflictingItems.Add(productName ?? $"Product ID {item.ProductId}");
                    }
                }

                if (conflictingItems.Any())
                {
                    return BadRequest(new
                    {
                        message = "The following products are already on sale during this selected period:",
                        products = conflictingItems
                    });
                }



                foreach (var item in model.Items)
                {
                    SaleModel sale = new()
                    {
                        ProductId = item.ProductId,
                        SalePrice = item.SalePrice,
                        SaleStartDate = model.SaleStartDate,
                        SaleEndDate = model.SaleEndDate,
                        Status = 1
                    };
                    await _context.Sales.AddAsync(sale);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, Message = "Add sale successful." });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = "Add sale fail. " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var saleModel = await _context.Sales.Include(x => x.Product)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (saleModel == null)
            {
                return NotFound();
            }
            EditSaleRequest editSale = _mapper.Map<EditSaleRequest>(saleModel);
            return PartialView("_EditSalePartial", editSale);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? Id,EditSaleRequest model)
        {

            if (Id == null)
            {
                return NotFound();
            }
            var saleModel = await _context.Sales.Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (saleModel == null)
            {
                return NotFound();
            }
            if (model.SalePrice >= saleModel.Product.Price)
                return Ok(new { success = false, Message = "The sale price cannot be higher than the selling price. " });

            if (model.SalePrice <= saleModel.Product.OriginalPrice)
                return Ok(new { success = false, Message = "The sale price cannot be less than the original price. " });

            try
            {

                if (ModelState.IsValid)
                {
                    saleModel.SalePrice = model.SalePrice;
                    _context.Sales.Update(saleModel);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, Message = "Edit sale successful" });
                }
                return Ok(new { success = false, Message = "Sale price must be a multiple of 1000." });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = "Edit sale fail " + ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {

            if (Id == null)
            {
                return NotFound();
            }
            var saleModel = await _context.Sales
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (saleModel == null)
            {
                return NotFound();
            }

            try
            {
                saleModel.Status = 0;
                _context.Sales.Update(saleModel);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, Message = "Remove sale successful" });

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = "Remove sale fail " + ex.Message });
            }
        }




    }
}
