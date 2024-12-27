using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles ="ADMIN")]
	public class ProductManageController : Controller
	{
		private readonly DShopContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUserModel> _userManager;

        public ProductManageController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
            _userManager = userManager;

        }
		public async Task<IActionResult> Index()
		{
			var products = await _dataContext.Products.Where(p => p.Status ==1)
															.Include(p => p.Category)
															.Include(p => p.Brand)
															.OrderByDescending(p => p.Id)
															.ToListAsync();

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
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "BrandName", product.BrandId);

			if(ModelState.IsValid)
			{
				product.Slug = product.ProductName.ToLower().Replace(" ", "-");
				var slug = await _dataContext.Products.FirstOrDefaultAsync(s => s.Slug == product.Slug);
				if(slug != null)
				{
					ModelState.AddModelError("", "Can't same slug");
					return View(product);
				}

				if(product.ImageUpload != null)
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
				await _dataContext.Products.AddAsync(product);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Add product success";
				return RedirectToAction("Index");
			}

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "BrandName", product.BrandId);

			return View(product);
            
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "BrandName", product.BrandId);

			var exitedProduct = await _dataContext.Products.FindAsync(Id);

            if (ModelState.IsValid)
            {
                product.Slug = product.ProductName.ToLower().Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(s => s.Slug == product.Slug);
                if (slug != null)
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
                    catch(Exception ex) 
					{
						ModelState.AddModelError("", "An error occurred while deleting the product image");
					}
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    exitedProduct.Image = imageName;

                }
                exitedProduct.ProductName = product.ProductName;
                exitedProduct.Slug = product.Slug;
				exitedProduct.Description = product.Description;
				exitedProduct.Price = product.Price;
				exitedProduct.CategoryId = product.CategoryId;
				exitedProduct.BrandId = product.BrandId;


                exitedProduct.Status = 1;
                _dataContext.Update(exitedProduct);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Update product success";
                return RedirectToAction("Index");
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


    }
}
