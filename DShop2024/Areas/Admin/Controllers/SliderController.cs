using AutoMapper;
using DShop2024.Areas.Admin.Models.Banner;
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
    [SidebarMenu(Menu.Admin.Blog, SubMenu.Blog.Slider)]
    public class SliderController : Controller
	{
		private readonly DShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        public SliderController(DShopContext context, IWebHostEnvironment webHostEnvironment, IMapper mapper)
		{
			_context = context;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
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
        public async Task<IActionResult> Create(CreateBannerRequest banner)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    var checkExit = await _context.Banners.Where( b => b.Status != 0 )
                                                           .Where( p => p.BannerName == banner.BannerName )
                                                          .AnyAsync();
                    if( checkExit)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "This banner already exists.";
                        return View(banner);
                    }

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
                    BannerModel bannerModel = _mapper.Map<BannerModel>(banner);
                    bannerModel.Status = 1;
                    await _context.Banners.AddAsync(bannerModel);
                    await _context.SaveChangesAsync();

                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Add banner success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Add banner fail "+ex.Message;
                    return View(banner);
                }    
            }
            return View(banner);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            
            if (Id == null)
            {
                return NotFound();
            }
            BannerModel bannerModel = await _context.Banners
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (bannerModel == null)
            {
                return NotFound();
            }
            return View(bannerModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, BannerModel banner)
        {
            
            if (Id != banner.Id)
            {
                return NotFound();
            }

            var exitedBanner = await _context.Banners.FindAsync(Id);

            if (ModelState.IsValid)
            {
                var checkExit = await _context.Banners.Where(b => b.Status != 0)
                                       .Where(p => p.BannerName == banner.BannerName)
                                      .AnyAsync();
                if (checkExit && banner.BannerName.ToLower() != exitedBanner.BannerName.ToLower())
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "This banner already exists.";
                    return View(exitedBanner);
                }

                if (banner.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");
                    string imageName = Guid.NewGuid().ToString() + "_" + banner.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    if (exitedBanner.Image != null)
                    {
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
                            TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the banner image file" + ex.Message;
                            return View(exitedBanner);
                        }
                    }

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await banner.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    exitedBanner.Image = imageName;

                }
                try
                {
                    exitedBanner.BannerName = banner.BannerName;
                    exitedBanner.Description = banner.Description;
                    _context.Update(exitedBanner);
                    await _context.SaveChangesAsync();

                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update banner success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Update banner fail "+ex.Message;
                    return View(exitedBanner);
                }

            }

            return View(exitedBanner);
        }

        public async Task<IActionResult> Delete(int? Id)
        {
            
            if (Id == null)
            {
                return NotFound();
            }
            BannerModel banner = await _context.Banners.FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (banner == null)
            {
                return NotFound();
            }

            
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
                    TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the banner image file" + ex.Message;
                    return RedirectToAction("Index");
                }
            }
            try
            {
                banner.Status = 0;
                _context.Banners.Update(banner);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Remove banner success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove banner fail " + ex.Message;
                return RedirectToAction("Index");

            }
        }


         
         
    }
}
