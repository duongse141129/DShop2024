using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.Catalog, SubMenu.Catalog.Category)]
    public class CategoryManageController : Controller
    {
        private readonly DShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryManageController(DShopContext context , IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int pg =1)
        {
            
            return View(await _context.Categories.Where(p => p.Status != 0).OrderByDescending(b => b.Id).ToListAsync());
        }



        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet]
        public IActionResult Create()
        {
            
            return View();
        }


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,Description,ImageUpload")] CategoryModel categoryModel)
        {
            
            if (ModelState.IsValid)
            {
                categoryModel.Slug = categoryModel.CategoryName.ToLower().Replace(" ", "-");
                var slug = await _context.Categories.FirstOrDefaultAsync(s => s.Slug == categoryModel.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This category already exists");
                    return View(categoryModel);
                }

                if (categoryModel.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/categories");
                    string imageName = Guid.NewGuid().ToString() + "_" + categoryModel.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await categoryModel.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    categoryModel.Image = imageName;

                }
                categoryModel.Status = 1;

                try
                {
                    _context.Add(categoryModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create category successful";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Delete category fail " + ex.Message;
                    return View(categoryModel);
                }

            }
            return View(categoryModel);
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var categoryModel = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (categoryModel == null)
            {
                return NotFound();
            }
            return View(categoryModel);
        }


        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryName,Description,ImageUpload")] CategoryModel categoryModel)
        {
            
            if (id != categoryModel.Id)
            {
                return NotFound();
            }
			var exitedCategory = await _context.Categories.FindAsync(id);
			if (ModelState.IsValid)
            {
                try
                {
					categoryModel.Slug = categoryModel.CategoryName.ToLower().Replace(" ", "-");
					var slug = await _context.Categories.FirstOrDefaultAsync(s => s.Slug == categoryModel.Slug);
					if (slug != null && exitedCategory.CategoryName.ToLower() != categoryModel.CategoryName.ToLower())
					{
						ModelState.AddModelError("", "This category already exists");
						return View(categoryModel);
					}

                    if (categoryModel.ImageUpload != null)
                    {

                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/categories");
                        string imageName = Guid.NewGuid().ToString() + "_" + categoryModel.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        if (exitedCategory.Image != null)
                        {
                            string oldFilePath = Path.Combine(uploadsDir, exitedCategory.Image);
                            try
                            {
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }

                            }
                            catch (Exception ex)
                            {
                                TempData[DShopConst.TEMPDATA_SUCCESS] = "Update image's category fail. " + ex.Message;
                            }
                        }

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await categoryModel.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        exitedCategory.Image = imageName;

                    }

                    exitedCategory.CategoryName = categoryModel.CategoryName;
					exitedCategory.Description = categoryModel.Description;
					exitedCategory.Slug = categoryModel.Slug;
					exitedCategory.Status = 1;

					_context.Update(exitedCategory);
					await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update category successful";
                }
                catch (DbUpdateConcurrencyException db)
                {
                    if (!CategoryModelExists(categoryModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "update category fail" + db.Message;
                        return View(categoryModel);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoryModel);
        }




        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var categoryModel = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (categoryModel == null)
            {
                return NotFound();
            }

            try
            {
                if (categoryModel.Image != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/categories");
                    string oldFilePath = Path.Combine(uploadsDir, categoryModel.Image);
                    try
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                    }
                    catch (Exception ex)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the category image " + ex.Message;
                        return RedirectToAction("Index");
                    }
                }

                categoryModel.Status = 0;
                _context.Update(categoryModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Delete category fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }

        private bool CategoryModelExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
