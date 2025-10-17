using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using DShop2024.EnumData;


namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class BrandManageController : Controller
    {
        private readonly DShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string sidebar = "brand";

        public BrandManageController(DShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = sidebar;
            return View(await _context.Brands.Where(p => p.Status != 0).ToListAsync());

        }

        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult Create()
        {
            ViewBag.sidebar = sidebar;
            return View();
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrandName,Description,ImageUpload")] BrandModel brandModel)
        {
            ViewBag.sidebar = sidebar;
            if (ModelState.IsValid)
            {
				brandModel.Slug = brandModel.BrandName.ToLower().Replace(" ", "-");
				var slug = await _context.Brands.FirstOrDefaultAsync(s => s.Slug == brandModel.Slug);
				if (slug != null)
				{
                    TempData[DShopConst.TEMPDATA_ERROR] = "This brand already exists.";
                    return View(brandModel);
				}

                if (brandModel.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
                    string imageName = Guid.NewGuid().ToString() + "_" + brandModel.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await brandModel.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    brandModel.Image = imageName;

                }
                brandModel.Status =1;

                try
                {
                    _context.Add(brandModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create brand successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {

                    TempData[DShopConst.TEMPDATA_ERROR] = "Create brand fail "+ ex.Message;
                    return View(brandModel);
                }
            }
            return View(brandModel);
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.sidebar = sidebar;
            if (id == null)
            {
                return NotFound();
            }

            var brandModel = await _context.Brands.FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (brandModel == null)
            {
                return NotFound();
            }
            return View(brandModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BrandName,Description,ImageUpload")] BrandModel brandModel)
        {
            ViewBag.sidebar = sidebar;
            if (id != brandModel.Id)
            {
                return NotFound();
            }
			var exitedBrand = await _context.Brands.FindAsync(id);
			if (ModelState.IsValid)
            {
                try
                {
					brandModel.Slug = brandModel.BrandName.ToLower().Replace(" ", "-");
					var slug = await _context.Brands.FirstOrDefaultAsync(s => s.Slug == brandModel.Slug);
					if (slug != null && exitedBrand.BrandName.ToLower() != brandModel.BrandName.ToLower())
					{
                        TempData[DShopConst.TEMPDATA_ERROR] = "This brand already exists.";
                        return View(brandModel);
					}

                    if (brandModel.ImageUpload != null)
                    {

                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
                        string imageName = Guid.NewGuid().ToString() + "_" + brandModel.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        if(exitedBrand.Image != null)
                        {
                            string oldFilePath = Path.Combine(uploadsDir, exitedBrand.Image);
                            try
                            {
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }

                            }
                            catch (Exception ex)
                            {
                                TempData[DShopConst.TEMPDATA_ERROR] = "update brand fail" + ex.Message;
                                return View(brandModel);
                            }
                        }
                     
                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await brandModel.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        exitedBrand.Image = imageName;

                    }

                    exitedBrand.BrandName = brandModel.BrandName;
					exitedBrand.Description = brandModel.Description;
					exitedBrand.Slug = brandModel.Slug;
					exitedBrand.Status = 1;

					_context.Update(exitedBrand);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update brand successful ";
                }
                catch (DbUpdateConcurrencyException db)
                {
                    if (!BrandModelExists(brandModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "update brand fail" + db.Message;
                        return View(brandModel);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brandModel);
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.sidebar = sidebar;
            if (id == null)
            {
                return NotFound();
            }
            var brandModel = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (brandModel == null)
            {
                return NotFound();
            }

            try
            {
                if (brandModel.Image != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
                    string oldFilePath = Path.Combine(uploadsDir, brandModel.Image);
                    try
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                    }
                    catch (Exception ex)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the brand image " + ex.Message;
                        return RedirectToAction("Index");
                    }
                }

                brandModel.Status = 0;
                _context.Brands.Update(brandModel);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete brand successful";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Delete brand fail "+ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }

        private bool BrandModelExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}
