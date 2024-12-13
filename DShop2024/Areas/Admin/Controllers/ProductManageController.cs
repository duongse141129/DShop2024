using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductManageController : Controller
	{
		private readonly DShopContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProductManageController(DShopContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
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
    }
}
