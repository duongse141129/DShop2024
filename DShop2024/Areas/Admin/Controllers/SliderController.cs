using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "ADMIN")]
	public class SliderController : Controller
	{
		private readonly DShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(DShopContext context, IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
            _webHostEnvironment = webHostEnvironment;
        }

		public async Task<IActionResult> Index()
		{
			return View(await _context.Banners.Where(p => p.Status != 0).ToListAsync());

		}

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BannerModel banner)
        {

            if (ModelState.IsValid)
            {
         

                if (banner.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");
                    string imageName = Guid.NewGuid().ToString() + "_" + banner.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await banner.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    banner.Image = imageName;

                }
                //banner.Status = 1;
                await _context.Banners.AddAsync(banner);
                await _context.SaveChangesAsync();

                TempData["success"] = "Add banner success";
                return RedirectToAction("Index");
            }

            return View(banner);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            BannerModel banner = await _context.Banners.FindAsync(Id);
            return View(banner);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, BannerModel banner)
        {

            var exitedBanner = await _context.Banners.FindAsync(Id);

            if (ModelState.IsValid)
            {
                if (banner.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");
                    string imageName = Guid.NewGuid().ToString() + "_" + banner.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    string oldFilePath = Path.Combine(uploadsDir, exitedBanner.Image);
                    try
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "An error occurred while deleting the banner image");
                    }
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await banner.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    exitedBanner.Image = imageName;

                }
                exitedBanner.BannerName = banner.BannerName;
                exitedBanner.Description = banner.Description;
                exitedBanner.Status = banner.Status;
                _context.Update(exitedBanner);
                await _context.SaveChangesAsync();

                TempData["success"] = "Update banner success";
                return RedirectToAction("Index");
            }

            return View(exitedBanner);
        }

        public async Task<IActionResult> Delete(int Id)
        {
            BannerModel banner = await _context.Banners.FindAsync(Id);
            if (!string.Equals(banner.Image, "noname.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");
                string oldFilePath = Path.Combine(uploadsDir, banner.Image);
                try
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while deleting the banner image");
                }
            }
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            TempData["success"] = "Remove banner success";
            return RedirectToAction("Index");

        }


         
         
    }
}
