using AutoMapper;
using DShop2024.Areas.Admin.Models.Product;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
	public class ProductManageController : Controller
	{
		private readonly DShopContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;
        private readonly string sidebar = "product";

        public ProductManageController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager, IMapper mapper)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _mapper = mapper;
        }


        public async Task<IActionResult> Index()
		{
            ViewBag.sidebar = sidebar;
            var products =  await _context.Products.Where(p => p.Status != 0)
                                                            .Include(p => p.Category)
                                                            .Include(p => p.Brand)
                                                            .OrderByDescending(p => p.Id).ToListAsync();
            return View(products);
		}

		[Authorize(Roles = RoleName.Administrator)]
		[HttpGet]
		public IActionResult Create()
		{
            ViewBag.sidebar = sidebar;
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName");
			ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName");

            ViewBag.laptopPocket = new SelectList(ProductEnumData.laptopPocketTypes, "");

            return View();
		}

		[Authorize(Roles = RoleName.Administrator)]
		[HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductRequest product)
        {
            ViewBag.sidebar = sidebar;
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName", product.BrandId);
            ViewBag.laptopPocket = new SelectList(ProductEnumData.laptopPocketTypes, product.LaptopPocket);

            if (ModelState.IsValid)
			{
                try
                {
                    if(product.OriginalPrice > product.Price)
                    {
                        ModelState.AddModelError("", "original price must <=  price");
                        return View(product);
                    }

                    product.Slug = product.ProductName.ToLower().Replace(" ", "-");
                    var slug = await _context.Products.FirstOrDefaultAsync(s => s.Slug == product.Slug);
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "This product already exists.");
                        return View(product);
                    }

                    if (product.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await product.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        product.Image = imageName;

                    }
                    product.Status = 1;
                    ProductModel productModel = _mapper.Map<ProductModel>(product);
                    await _context.Products.AddAsync(productModel);
                    await _context.SaveChangesAsync();

                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Add product success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while deleting the product image " + ex.Message);
                }
				
			}

            return View(product);
        }

		[Authorize(Roles = RoleName.Administrator)]
		[HttpGet]
        public async Task<IActionResult> Edit(int? Id)
		{
            ViewBag.sidebar = sidebar;
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName", product.BrandId);

            ViewBag.laptopPocket = new SelectList(ProductEnumData.laptopPocketTypes, product.LaptopPocket.ToString());

            UpdateProductRequest updateProduct = _mapper.Map<UpdateProductRequest>(product);
            return View(updateProduct);
            
		}

		[Authorize(Roles = RoleName.Administrator)]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, UpdateProductRequest product)
        {
            ViewBag.sidebar = sidebar;
            if (Id != product.Id)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName", product.BrandId);

			var exitedProduct = await _context.Products.FindAsync(Id);

            if (ModelState.IsValid)
            {
                try
                {
                    if (product.OriginalPrice > product.Price)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Original price must <=  price";
                        return View(product);
                    }

                    product.Slug = product.ProductName.ToLower().Replace(" ", "-");
                    var slug = await _context.Products.FirstOrDefaultAsync(s => s.Slug == product.Slug);
                    if (slug != null && product.Slug.ToLower() != exitedProduct.Slug.ToLower())
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "This product already exists.";
                        return View(product);
                    }

                    if (product.ImageUpload != null)
                    {

                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        if(exitedProduct.Image != null)
                        {
                            string oldFilePath = Path.Combine(uploadsDir, exitedProduct.Image);
                            try
                            {
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }

                            }
                            catch (Exception ex)
                            {
                                TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the product image "+ex.Message;
                                return View(product);
                            }
                        }
                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await product.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        exitedProduct.Image = imageName;

                    }

                    exitedProduct = _mapper.Map(product, exitedProduct);      
                    _context.Update(exitedProduct);
                    await _context.SaveChangesAsync();

                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update product success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Edit product fail " + ex.Message;
                    return View(product);
                }
               
            }

            return View(exitedProduct);
        }

		[Authorize(Roles = RoleName.Administrator)]
		public async Task<IActionResult> Delete(int? Id)
		{
            ViewBag.sidebar = sidebar;
            if (Id == null)
            {
                return NotFound();
            }
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }
            try
            {
                await DeleteProduct(product.Id);
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Remove product successful";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove product fail "+ex.Message;
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task DeleteProduct(int? Id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);

            if (!String.IsNullOrEmpty(product.Image))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldFilePath = Path.Combine(uploadsDir, product.Image);
                try
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Remove image product fail " + ex.Message;
                }
            }
            try
            {
                product.Status = 0;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove product fail " + ex.Message;
            }
        }


        [Authorize(Roles = RoleName.Administrator)]
		public  async Task<IActionResult> DeleteMultiple(List<int> IdProductsToDelete)
        {
            ViewBag.sidebar = sidebar;
            if (IdProductsToDelete.Count == 0)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Select list product to delete mutiple" ;
                return RedirectToAction("Index");
            }
            try
            {
                foreach (int idProduct in IdProductsToDelete)
                {
                    await DeleteProduct(idProduct);
                }
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete Multiple product success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                TempData[DShopConst.TEMPDATA_ERROR] = "Delete Multiple product fail "+ ex.Message;
                return RedirectToAction("Index");
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> AddQuantity(int? Id)
        {
            ViewBag.sidebar = sidebar;
            if (Id == null)
            {
                return NotFound();
            }
            var productModel = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (productModel == null)
            {
                return NotFound();
            }

            var receivingStockList = await _context.ReceivingStocks.Where(x => x.ProductId == Id && x.Status != 0)
                                                .Include(p => p.Product)
                                                .Include(r => r.User)
                                                .ToListAsync();
            ViewBag.receivingStockList = receivingStockList;
            ViewBag.totalQuantity = receivingStockList.Sum(r => r.Quantity);
            ViewBag.totalOriginalPrice = receivingStockList.Sum(r => r.Product.OriginalPrice);
            ViewBag.product = productModel;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreProductQuantity(ReceivingStockModel receivingStock)
        {
            ViewBag.sidebar = sidebar;
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == receivingStock.ProductId && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }


            try
            {
                product.Stock += receivingStock.Quantity;

                var user = await _userManager.GetUserAsync(this.User);

                receivingStock.Quantity = receivingStock.Quantity;
                receivingStock.ProductId = receivingStock.ProductId;
                receivingStock.DateReceive = DateTime.Now;
                receivingStock.UserId = user.Id;
                receivingStock.Status = 1;

                _context.ReceivingStocks.Add(receivingStock);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = $"Add quantity product: {product.ProductName} successful";
                return RedirectToAction("AddQuantity", "ProductManage", new { Id = receivingStock.ProductId });
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = $"Add quantity product: {product.ProductName} fail "+ ex.Message;
                return RedirectToAction("AddQuantity", "ProductManage", new { Id = receivingStock.ProductId });
            }


        }

		public async Task<IActionResult> Detail(int? id, [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
		{
            ViewBag.sidebar = sidebar;
            if (id == null)
			{
				return NotFound();
			}

			var productModel = await _context.Products.Include( b => b.Brand).Include( c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (productModel == null)
			{
				return NotFound();
			}


			var listRating =  _context.Ratings
						.Where(p => p.ProductId == id)
						.Where(r => r.Status == 1)
						.Include(c => c.User);
			var pointAvarge = 0.0;
            List<RatingModel> ratings = new List<RatingModel>();

            var count = await listRating.CountAsync();
			if (count > 0)
			{
				pointAvarge = listRating.Average(p => p.Star);


				int totalRating = listRating.Count();
				if (pagesSize <= 0)
					pagesSize = 5;
				int countPages = (int)Math.Ceiling((double)totalRating / 5);

				if (currentPage > countPages)
					currentPage = countPages;
				if (currentPage < 1)
					currentPage = 1;

				var pagingModel = new PagingModel()
				{
					countpages = countPages,
					currentpage = currentPage,
					generateUrl = (pageNumber) => Url.Action("Detail", new
					{
						p = pageNumber,
						pagesSize = pagesSize,
						id = id
					})
				};

				ratings = await listRating
							.Skip((currentPage - 1) * pagesSize)
							.Take(pagesSize).ToListAsync();

				ViewBag.pagingModel = pagingModel;

			}

			var viewModel = new ProductDetailViewModel
			{
				ProductDetail = productModel,
				Point = pointAvarge,
				listRating = ratings
			};


			return View(viewModel);
		}


	}
}
