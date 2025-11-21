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
using System.IO;

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

        public ProductManageController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager, IMapper mapper)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _mapper = mapper;
        }


        public async Task<IActionResult> Index()
		{
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
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
                            await _context.SaveChangesAsync();
                        }

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
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
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
                var listImage = await _context.ProductImages.Where( p => p.ProductId == product.Id ).ToListAsync();
                if(listImage.Count > 0)
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
		public  async Task<IActionResult> DeleteMultiple(List<int> IdProductsToDelete)
        {
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
            if (id == null)
			{
				return NotFound();
			}

			var productModel = await _context.Products
                                        .Include( b => b.Brand)
                                        .Include( c => c.Category)
                                        .Include( d => d.Images)
                                        .AsSplitQuery()
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (productModel == null)
			{
				return NotFound();
			}

			var listRating =  _context.Ratings
						.Where(p => p.ProductId == id)
						.Where(r => r.Status == 1)
						.Include(c => c.User)
						.Include(c => c.ReplyBy)
                        .OrderByDescending(c => c.RatingDateTime);
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
				listRating = ratings,
                ExistingImages = productModel.Images.Select(i => i.ImagePath).ToList()
            };


			return View(viewModel);
		}


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImages(int productId, List<IFormFile> ImageFiles)
        {
            ViewBag.sidebar = Menu.Admin.Product;
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
            ViewBag.sidebar = Menu.Admin.Product;
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

        [HttpGet]
        public async Task<IActionResult> ManageRating([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            var ListRating =  _context.Ratings.Where( r => r.Status != 0)
                                    .Include(r => r.Product)
                                    .Include(r => r.User)
                                    .Include(r => r.ReplyBy)
                                    .OrderByDescending(r => r.RatingDateTime);
            int totalRating = ListRating.Count();
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
                generateUrl = (pageNumber) => Url.Action("ManageRating", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };

            var ratings = await ListRating.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize).ToListAsync();

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
            var ratingModel = await _context.Ratings
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

        [HttpGet]
        public async Task<IActionResult> RemoveRatingPopup(int? Id)
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
            return PartialView("_ModalRemoveRatingPartial", ratingModel);
        }

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
