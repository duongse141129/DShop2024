using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class InformationController : Controller
	{
		private readonly DShopContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string sidebar = "information";

        public InformationController(DShopContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;

		}
		public IActionResult Index()
		{
            ViewBag.sidebar = sidebar;
            var info = _dataContext.InformationShops.FirstOrDefault();
			return View(info);
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

            InformationShopModel info = await _dataContext.InformationShops.FindAsync(Id);
            if (info == null)
            {
                return NotFound();
            }
            return View(info);

		}

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int Id, InformationShopModel informationShop)
		{
            ViewBag.sidebar = sidebar;
            if (Id != informationShop.Id)
			{
				return NotFound();
			}

			var exitedInformationShop = await _dataContext.InformationShops.FindAsync(Id);
			if (ModelState.IsValid)
			{
				try
				{
					if (informationShop.ImageUpload != null)
					{

						string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/Logo");
						string imageName = Guid.NewGuid().ToString() + "_" + informationShop.ImageUpload.FileName;
						string filePath = Path.Combine(uploadsDir, imageName);


						FileStream fs = new FileStream(filePath, FileMode.Create);
						await informationShop.ImageUpload.CopyToAsync(fs);
						fs.Close();
						exitedInformationShop.LogoImg = imageName;

					}
					exitedInformationShop.ShopName = informationShop.ShopName;
					exitedInformationShop.Description = informationShop.Description;
					exitedInformationShop.Map = informationShop.Map;
					exitedInformationShop.Address = informationShop.Address;
					exitedInformationShop.Phone = informationShop.Phone;
					exitedInformationShop.Email = informationShop.Email;
					exitedInformationShop.PluginFacebook = informationShop.PluginFacebook;
					exitedInformationShop.PluginYoutube = informationShop.PluginYoutube;

					_dataContext.Update(exitedInformationShop);
					await _dataContext.SaveChangesAsync();

					TempData[DShopConst.TEMPDATA_SUCCESS] = "Update Information shop successful";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					TempData[DShopConst.TEMPDATA_ERROR] = "Update Information shop fail "+ex.Message;
					return RedirectToAction("Index");
				}
			}

			return View(exitedInformationShop);
		}
	}
}
