using AutoMapper;
using DShop2024.Areas.Admin.Models.Product;
using DShop2024.Areas.Admin.Models.Sale;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.Catalog, SubMenu.Catalog.Product)]
    public class ProductManageController : Controller
	{
		private readonly DShopContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;

        public ProductManageController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager, IMapper mapper)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string search = "", string brand_by = "", string category_by = "", bool isSale = false,
                              string sortColumn = "Id",
                              string sortOrder = "desc",
                                [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            if (User.IsInRole(RoleName.Employee))
            {
                return RedirectToAction("ViewProducts");
            }

            var now = DateTime.Now;
            var query = _context.Products
                .Where(p => p.Status != 0)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.Slug,
                    p.MainImage,
                    p.Price,
                    p.OriginalPrice,
                    p.Stock,
                    p.BrandId,
                    BrandName = p.Brand.BrandName,
                    p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    AveragePoint = p.Ratings.Any(r => r.Status != 0) ? p.Ratings.Where(r => r.Status != 0).Average(r => r.Star) : 0,
                    QuantitySold = p.OrderDetails.Where(od => od.Order.Status != 0).Sum(od => (int?)od.Quantity) ?? 0,
                    WishlistCount = p.WishLists.Count(),
                    IsOnSale = p.Sales.Any(s => s.Status != 0 && now >= s.SaleStartDate && now <= s.SaleEndDate),
                    SalePrice = p.Sales.Where(s => s.Status != 0 && now >= s.SaleStartDate && now <= s.SaleEndDate)
                                        .OrderByDescending(s => s.SaleStartDate)
                                        .Select(s => (decimal?)s.SalePrice)
                                        .FirstOrDefault()
                });

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search));
            if (!string.IsNullOrEmpty(brand_by))
                query = query.Where(p => p.BrandId == int.Parse(brand_by));
            if (!string.IsNullOrEmpty(category_by))
                query = query.Where(p => p.CategoryId == int.Parse(category_by));
            if (isSale)
                query = query.Where(p => p.IsOnSale);

            query = (sortColumn, sortOrder) switch
            {
                ("ProductName", "asc") => query.OrderBy(p => p.ProductName),
                ("ProductName", _) => query.OrderByDescending(p => p.ProductName),
                ("Price", "asc") => query.OrderBy(p => p.IsOnSale ? p.SalePrice : p.Price),
                ("Price", _) => query.OrderByDescending(p => p.IsOnSale ? p.SalePrice : p.Price),
                ("OriginalPrice", "asc") => query.OrderBy(p => p.OriginalPrice),
                ("OriginalPrice", _) => query.OrderByDescending(p => p.OriginalPrice),
                ("Brand", "asc") => query.OrderBy(p => p.BrandName),
                ("Brand", _) => query.OrderByDescending(p => p.BrandName),
                ("Category", "asc") => query.OrderBy(p => p.CategoryName),
                ("Category", _) => query.OrderByDescending(p => p.CategoryName),
                ("Stock", "asc") => query.OrderBy(p => p.Stock),
                ("Stock", _) => query.OrderByDescending(p => p.Stock),
                ("AveragePoint", "asc") => query.OrderBy(p => p.AveragePoint),
                ("AveragePoint", _) => query.OrderByDescending(p => p.AveragePoint),
                ("QuantitySold", "asc") => query.OrderBy(p => p.QuantitySold),
                ("QuantitySold", _) => query.OrderByDescending(p => p.QuantitySold),
                ("WishlistCount", "asc") => query.OrderBy(p => p.WishlistCount),
                ("WishlistCount", _) => query.OrderByDescending(p => p.WishlistCount),
                ("Id", "asc") => query.OrderBy(p => p.Id),
                _ => query.OrderByDescending(p => p.Id),
            };

            int totalProduct = await query.CountAsync();
            if (pagesSize <= 0) pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalProduct / pagesSize);
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize,
                    search = search,
                    brand_by = brand_by,
                    category_by = category_by,
                    isSale= isSale,
                    sortColumn = sortColumn,
                    sortOrder = sortOrder
                })
            };

            var pageData = await query.Skip((currentPage - 1) * pagesSize)
                                       .Take(pagesSize)
                                       .ToListAsync();

            var productVMs = pageData.Select(p => new ProductListViewModel
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Slug = p.Slug,
                MainImage = p.MainImage,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Stock = p.Stock,
                BrandName = p.BrandName,
                CategoryName = p.CategoryName,
                AveragePoint = p.AveragePoint,
                QuantitySold = p.QuantitySold,
                WishlistCount = p.WishlistCount,
                SalePrice = p.SalePrice,
                IsOnSale = p.IsOnSale
            }).ToList();

            ViewBag.pagingModel = pagingModel;
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName");
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName");
            ViewBag.Search = search;
            ViewBag.PageSize = pagesSize;
            ViewBag.IsSale = isSale;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductListPartial", productVMs);
            }

            return View(productVMs);
        }


        public async Task<IActionResult> ViewProducts()
        {
            var products = await _context.Products.Where(b => b.Status != 0)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Ratings)
                .Include(p => p.Sales)
                .ToListAsync();
            var productVMs = _mapper.Map<List<ProductViewModel>>(products);
            return View(productVMs);
        }



        [Authorize(Roles = RoleName.Administrator)]
		[HttpGet]
		public IActionResult Create()
		{
            
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
                        product.MainImage = imageName;

                    }
                    product.Status = 1;
                    product.CreateDate = DateTime.Now;
                    ProductModel productModel = _mapper.Map<ProductModel>(product);
                    var createdProduct = await _context.Products.AddAsync(productModel);
                    await _context.SaveChangesAsync();

                    if(createdProduct != null && product.ImageFiles.Count > 0)
                    {
                        string imgagePath = $"media/imageProduct/productId{productModel.Id}";
                        var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, imgagePath);
                        if (!Directory.Exists(uploadDirectory))
                        {
                            Directory.CreateDirectory(uploadDirectory);
                        }
                        foreach (var file in product.ImageFiles)
                        {
                            string imageProductName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            var filePath = Path.Combine(uploadDirectory, imageProductName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            ProductImageModel productImage = new ProductImageModel {
                                ImagePath = imgagePath + "/" +imageProductName,
                                ProductId = productModel.Id,
                            };
                            await _context.ProductImages.AddAsync(productImage);
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Add product success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the product " + ex.Message);
                }
				
			}

            return View(product);
        }

		[Authorize(Roles = RoleName.Administrator)]
		[HttpGet]
        public async Task<IActionResult> Edit(int? Id)
		{
            
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName", product.BrandId);

            ViewBag.laptopPocket = new SelectList(ProductEnumData.laptopPocketTypes, product.LaptopPocket.ToString());

            UpdateProductRequest updateProduct = _mapper.Map<UpdateProductRequest>(product);
            updateProduct.ExistingImages = product.Images.ToList();
            return View(updateProduct);
            
		}

		[Authorize(Roles = RoleName.Administrator)]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, UpdateProductRequest product)
        {
            
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

                        if(exitedProduct.MainImage != null)
                        {
                            string oldFilePath = Path.Combine(uploadsDir, exitedProduct.MainImage);
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
                        exitedProduct.MainImage = imageName;

                    }

                    exitedProduct = _mapper.Map(product, exitedProduct);      
                    _context.Products.Update(exitedProduct);
                    await _context.SaveChangesAsync();

                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update product successful";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Edit product fail " + ex.Message;
                    return View(product);
                }
               
            }

            return View(product);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? Id)
        {

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
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove product fail " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task DeleteProduct(int? Id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);

            if (!String.IsNullOrEmpty(product.MainImage))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldFilePath = Path.Combine(uploadsDir, product.MainImage);
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

                var listImage = await _context.ProductImages.Where(p => p.ProductId == product.Id).ToListAsync();
                if (listImage.Count > 0)
                {
                    foreach (var item in listImage)
                    {
                        await DeleteImage(item.Id);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove product fail " + ex.Message;
            }
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeleteMultiProductPopup(List<int> IdProductsToDelete)
        {
            if (IdProductsToDelete.Count == 0)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Select list product to delete mutiple";
                return RedirectToAction("Index");
            }
            List<ProductModel> products = new List<ProductModel>();
            foreach (var item in IdProductsToDelete)
            {
                var product = await _context.Products.Where(p => p.Status != 0 && p.Id == item).FirstOrDefaultAsync();
                products.Add(product);
            }

            return PartialView("_DeleteMultiProductPopUpParital", products);
        }




        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
		public  async Task<IActionResult> DeleteMultiple(List<int> IdProductsToDelete)
        {
            
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
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete Multiple product successful";
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
            ViewBag.totalOriginalPrice = receivingStockList.Sum(r => r.Quantity) * productModel.OriginalPrice;
            ViewBag.product = productModel;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreProductQuantity(ReceivingStockModel receivingStock)
        {
            
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
            
            if (id == null)
			{
				return NotFound();
			}

			var productModel = await _context.Products
                                        .Include( b => b.Brand)
                                        .Include( c => c.Category)
                                        .Include( d => d.Images)
                                        .Include( d => d.Sales)
                                        .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (productModel == null)
			{
				return NotFound();
			}

			var listRating =  _context.Ratings
						.Where(p => p.ProductId == id)
						.Where(r => r.Status != 0)
                        .OrderByDescending(c => c.RatingDateTime).AsQueryable();
			var pointAvarge = 0.0;
            List<RatingViewModel> ratings = new List<RatingViewModel>();

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
							.Take(pagesSize)
                            .Include(c => c.User)
                            .Include(c => c.ReplyBy)
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

			}
            var now = DateTime.Now;
            var activeSale = productModel.Sales?.FirstOrDefault(s => s.Status != 0 && s.SaleStartDate <= now && s.SaleEndDate >= now);
            var viewModel = new ProductDetailManageViewModel
            {
				ProductDetail = productModel,
				Point = pointAvarge,
				listRating = ratings,
                ConutTotalFeedBack = count,
                ExistingImages = productModel.Images.Select(i => i.ImagePath).ToList(),
                IsOnSale = activeSale != null,
                SalePrice = activeSale?.SalePrice
            };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_FeedbackListAdminPartial", viewModel);
            }

            return View(viewModel);
		}


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImages(int productId, List<IFormFile> ImageFiles)
        {
            
            ProductModel product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == productId && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName", product.BrandId);

            if (ModelState.IsValid)
            {
                try
                {

                    if ( ImageFiles.Count > 0)
                    {
                        string imgagePath = $"media/imageProduct/productId{product.Id}";
                        var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, imgagePath);
                        if (!Directory.Exists(uploadDirectory))
                        {
                            Directory.CreateDirectory(uploadDirectory);
                        }
                        foreach (var file in ImageFiles)
                        {
                            string imageProductName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            var filePath = Path.Combine(uploadDirectory, imageProductName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            ProductImageModel productImage = new ProductImageModel
                            {
                                ImagePath = imgagePath + "/" + imageProductName,
                                ProductId = product.Id,
                            };
                            await _context.ProductImages.AddAsync(productImage);
                            await _context.SaveChangesAsync();                        
                        }
                        TempData[DShopConst.TEMPDATA_SUCCESS] = "Add images product successful ";
                        return RedirectToAction("Edit", new { Id = product.Id });
                    }

                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Add images product fail " + ex.Message;
                    return RedirectToAction("Edit", new { Id = product.Id });
                }
            }

            return RedirectToAction("Edit", new { Id = product.Id});
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeleteImage(int? Id)
        {

            if (Id == null)
            {
                return NotFound();
            }
            var productImage = await _context.ProductImages.FindAsync(Id);
            if (productImage == null)
            {
                return NotFound();
            }
            try
            {
                if (!String.IsNullOrEmpty(productImage.ImagePath))
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, productImage.ImagePath);
                    if (System.IO.File.Exists(uploadsDir))
                    {
                        System.IO.File.Delete(uploadsDir);
                    }
                }

                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Remove product's image successful";
                return RedirectToAction("Edit", new { Id = productImage.ProductId });
            }
            catch (IOException io)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove product's image file fail. " + io.Message;
                return RedirectToAction("Edit", new { Id = productImage.ProductId });
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove product's image fail " + ex.Message;
                return RedirectToAction("Edit", new { Id = productImage.ProductId });
            }
        }




        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet]
        public async Task<IActionResult> GetPopUpChangeSale(int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }
            var product = await _context.Products.Where(p => p.Status != 0 && p.Id == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }
            var now = DateTime.Now;
            var saleModel = await _context.Sales.Include(x => x.Product).Where( s => s.Status != 0 && s.ProductId == product.Id && now >= s.SaleStartDate && now <= s.SaleEndDate)
                .FirstOrDefaultAsync();
            if (saleModel == null)
            {
                ViewBag.ProductId = product.Id;
                ViewBag.ProductName = product.ProductName;
                ViewBag.MainImage = product.MainImage;
                ViewBag.OriginalPrice = product.OriginalPrice;
                ViewBag.Price = product.Price;
                ViewBag.StartDate = now;
                return PartialView("_AddSalePartial");
            }
            EditSaleRequest editSale = _mapper.Map<EditSaleRequest>(saleModel);
            return PartialView("_EditSaleProductPartial", editSale);
        }


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        public async Task<IActionResult> AddSale( CreateSaleRequest model)
        {
            var product = await _context.Products.Where(p => p.Status != 0 && p.Id == model.ProductId).FirstOrDefaultAsync();
            if (product == null)
            {
                return Ok(new { success = false, Message = "Product does not exist." });
            }

            if (model.SaleStartDate >= model.SaleEndDate)
                return Ok(new { success = false, Message = "Start date must be earlier than End date." });

            if (model.SalePrice >= product.Price)
                return Ok(new { success = false, message = "The sale price cannot be higher than the selling price. " });

            if (model.SalePrice <= product.OriginalPrice)
                return Ok(new { success = false, message = "The sale price cannot be less than the original price. " });

            try
            {
                bool hasConflict = await _context.Sales.Where(s => s.Status != 0).AnyAsync(s =>
                    s.ProductId == model.ProductId &&
                    s.SaleStartDate < model.SaleEndDate &&
                    s.SaleEndDate > model.SaleStartDate);

                if (hasConflict)
                {
                    return Ok(new { success = false, Message = $"Product {product.ProductName} is already on sale during this selected period." });
                }

                if (ModelState.IsValid)
                {
                    SaleModel sale = new()
                    {
                        ProductId = model.ProductId,
                        SalePrice = model.SalePrice,
                        SaleStartDate = model.SaleStartDate,
                        SaleEndDate = model.SaleEndDate,
                        Status = 1
                    };
                    await _context.Sales.AddAsync(sale);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, Message = "Add sale successful." });
                }
                return Ok(new { success = false, Message = "Sale price must be a multiple of 1000." });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = "Add sale fail. " + ex.Message });
            }
        }


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        public async Task<IActionResult> EditSalePrice(int? Id, EditSaleRequest model)
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
                return Ok(new { success = false, message = "The sale price cannot be higher than the selling price. " });

            if (model.SalePrice <= saleModel.Product.OriginalPrice)
                return Ok(new { success = false, message = "The sale price cannot be less than the original price. " });

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


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        public async Task<IActionResult> DeleteSale(int? Id)
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
