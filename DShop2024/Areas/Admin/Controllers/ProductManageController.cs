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
using static DShop2024.EnumData.Product;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles ="ADMIN")]
	public class ProductManageController : Controller
	{
		private readonly DShopContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;

        public ProductManageController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager, IMapper mapper)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _mapper = mapper;
        }
		public async Task<IActionResult> Index()
		{
            var products =  await _dataContext.Products.Where(p => p.Status != 0)
                                                            .Include(p => p.Category)
                                                            .Include(p => p.Brand)
                                                            .OrderByDescending(p => p.Id).ToListAsync();
            return View(products);
		}

		[HttpGet]
		public IActionResult Create()
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "CategoryName");
			ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "BrandName");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductRequest product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "BrandName", product.BrandId);

			if(ModelState.IsValid)
			{
                try
                {
                    if(product.OriginalPrice > product.Price)
                    {
                        ModelState.AddModelError("", "original price must <=  price");
                        return View(product);
                    }

                    product.Slug = product.ProductName.ToLower().Replace(" ", "-");
                    var slug = await _dataContext.Products.FirstOrDefaultAsync(s => s.Slug == product.Slug);
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Can't same slug. Product Name is exited");
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
                    await _dataContext.Products.AddAsync(productModel);
                    await _dataContext.SaveChangesAsync();

                    TempData["success"] = "Add product success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while deleting the product image " + ex.Message);
                }
				
			}

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "BrandName", product.BrandId);

            List<string> strings = new List<string> { "", "14.00", "15.60", "17.30" };
            ViewBag.laptopPocket = new SelectList(strings, product.LaptopPocket.ToString());

            UpdateProductRequest updateProduct = _mapper.Map<UpdateProductRequest>(product);
            return View(updateProduct);
            
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, UpdateProductRequest product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "BrandName", product.BrandId);

			var exitedProduct = await _dataContext.Products.FindAsync(Id);

            if (ModelState.IsValid)
            {
                try
                {
                    if (product.OriginalPrice > product.Price)
                    {
                        ModelState.AddModelError("", "original price must <=  price");
                        return View(product);
                    }

                    product.Slug = product.ProductName.ToLower().Replace(" ", "-");
                    var slug = await _dataContext.Products.FirstOrDefaultAsync(s => s.Slug == product.Slug);
                    if (slug != null && product.Slug != exitedProduct.Slug)
                    {
                        ModelState.AddModelError("", "Can't same slug");
                        return View(product);
                    }

                    if (product.ImageUpload != null)
                    {

                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

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
                            ModelState.AddModelError("", "An error occurred while deleting the product image");
                        }
                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await product.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        exitedProduct.Image = imageName;

                    }

                    exitedProduct = _mapper.Map(product, exitedProduct);      
                    _dataContext.Update(exitedProduct);
                    await _dataContext.SaveChangesAsync();

                    TempData["success"] = "Update product success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while deleting the product image " + ex.Message);
                }
               
            }

            return View(exitedProduct);
        }


        public async Task<IActionResult> Delete(int Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			if(!string.Equals(product.Image, "noname.jpg"))
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
                    ModelState.AddModelError("", "An error occurred while deleting the product image");
                }
            }
			_dataContext.Products.Remove(product);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Remove product success";
            return RedirectToAction("Index");

        }


        public  async Task<IActionResult> DeleteMultiple(List<int> IdProductsToDelete)
        {
            if(IdProductsToDelete.Count == 0)
            {
                TempData["success"] = "Select list product to delete mutiple" ;
                return RedirectToAction("Index");
            }
            try
            {
                foreach (int idProduct in IdProductsToDelete)
                {
                    var product = await _dataContext.Products.FindAsync(idProduct);
                    product.Status = 0;
                    _dataContext.Products.Update(product);
                    await _dataContext.SaveChangesAsync();
                }
                TempData["success"] = "Delete Multiple product success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                TempData["error"] = "Delete Multiple product fail "+ ex.Message;
                return RedirectToAction("Index");
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var receivingStockList = await _dataContext.ReceivingStocks.Where(x => x.ProductId == Id).Include(r => r.User).ToListAsync();
            ViewBag.receivingStockList = receivingStockList;
            ViewBag.Id = Id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreProductQuantity(ReceivingStockModel receivingStock)
        {
            var product = await _dataContext.Products.FindAsync(receivingStock.ProductId);
            if(product == null)
            {
                return NotFound();
            }
            product.Stock += receivingStock.Quantity;

            var user = await _userManager.GetUserAsync(this.User);

            receivingStock.Quantity = receivingStock.Quantity;
            receivingStock.ProductId = receivingStock.ProductId;
            receivingStock.DateReceive = DateTime.Now;
            receivingStock.UserId = user.Id;
            receivingStock.Status = 1;

            _dataContext.ReceivingStocks.Add(receivingStock);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = $"Add quantity product: {product.ProductName} successful";
            return RedirectToAction("AddQuantity", "ProductManage", new { Id = receivingStock.ProductId });



        }

		public async Task<IActionResult> Detail(int? id, [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
		{
			if (id == null)
			{
				return NotFound();
			}

			var productModel = await _dataContext.Products.Include( b => b.Brand).Include( c => c.Category)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (productModel == null)
			{
				return NotFound();
			}


			var listRating =  _dataContext.Ratings
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
