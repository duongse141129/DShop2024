using App.Areas.Identity.Models.ManageViewModels;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Areas.Identity.Controllers
{

    [Authorize]
    [Area("Identity")]
    [Route("/Member/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ManageController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ManageController(
        UserManager<AppUserModel> userManager,
        SignInManager<AppUserModel> signInManager,
        IEmailSender emailSender,
        IWebHostEnvironment webHostEnvironment,
        ILogger<ManageController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }


        private Task<AppUserModel> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction("EditProfile", "Manage");
                }
                ModelState.AddModelError(result);
                return View(model);
            }
            return RedirectToAction("EditProfile", "Manage");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfileAsync()
        {
            var user = await GetCurrentUserAsync();
            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditExtraProfileModel()
            {
                BirthDate = user.BirthDate,
                UserName = user.UserName,
                UserEmail = user.Email,
                PhoneNumber = user.PhoneNumber,
                Occupation = user.Occupation,
                Avatar = user.Avatar,
                Gender = user.Gender,
                LoginType = user.LoginType,
                Address = user.Address,
                CustomerSegment = ((UserEnumData.StatusCustomerSegment)user.CustomerSegment).ToString(),
                RoleName = roles.FirstOrDefault()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileAsync(EditExtraProfileModel model)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                user.PhoneNumber = model.PhoneNumber;
                user.BirthDate = model.BirthDate;
                user.Occupation = model.Occupation;
                user.Gender = model.Gender;

                if (model.AvatarUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/avatar");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.AvatarUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    string oldFilePath = Path.Combine(uploadsDir, user.Avatar);
                    try
                    {
                        if (System.IO.File.Exists(oldFilePath) && user.Avatar != UserEnumData.IMAGE_DEFAULT)
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                    }
                    catch (Exception ex)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the product image " + ex.Message;
                        return RedirectToAction("EditProfile", "Manage");
                    }
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await model.AvatarUpload.CopyToAsync(fs);
                    fs.Close();
                    user.Avatar = imageName;
                }

                await _userManager.UpdateAsync(user);
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Edit profile successful" ;
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("EditProfile", "Manage");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Edit profile fail "+ ex.Message;
                return RedirectToAction("EditProfile", "Manage");
            }

        }

        [HttpGet]
        public async Task<IActionResult> EditAddress()
        {
            var user = await GetCurrentUserAsync();
            ViewBag.address = user.Address;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditAddress(string tinh, string quan, string phuong, string street)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                user.Address = $"{street}_{phuong}_{quan}_{tinh}";
                await _userManager.UpdateAsync(user);
                return Ok(new { success = true, Message = "Edit address successful " });
            }
            return Ok(new { success = false, Message = "Edit address fail " });
        }

    }
}
