using System.Data;
using System.Security.Claims;
using System.Text;
using App.Areas.Identity.Models.AccountViewModels;
using App.Utilities;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Identity.Controllers
{
    [Authorize]
    [Area("Identity")]
    [Route("/Account/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;
        private readonly DShopContext _context;

        public AccountController(
            UserManager<AppUserModel> userManager,
            SignInManager<AppUserModel> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            DShopContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _context = context;
        }

        // GET: /Account/Login
        //[HttpGet("/login/")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        //[HttpPost("/login/")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var checkUserNameExit = await _context.Users.AnyAsync(x => x.UserName == model.UserNameOrEmail);
                var checkEmailExit = await  _context.Users.AnyAsync(y => y.Email == model.UserNameOrEmail);           

                if (checkUserNameExit|| checkEmailExit)
                {
                    AppUserModel user = null;
                    user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
                    if(user == null)
                    {
                        user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
                    }
                    if(user.Status == 0)
                    {
                        ModelState.AddModelError("Account was deleted ");
                        return View(model);
                    }
                    var result = await _signInManager.PasswordSignInAsync(model.UserNameOrEmail, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if ((!result.Succeeded) && AppUtilities.IsValidEmail(model.UserNameOrEmail))
                    {
                        result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    }

                    if (result.Succeeded)
                    {
                        _logger.LogInformation(1, "User logged in.");
                        var roleUser = await _userManager.GetRolesAsync(user);
                        if (roleUser.Count > 0)
                        {
                            if (roleUser.FirstOrDefault() == RoleName.Administrator)
                            {
                                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                            }
                            else if (roleUser.FirstOrDefault() == RoleName.Employee)
                            {
                                return RedirectToAction("Index", "Order", new { area = "Admin" });
                            }
                        }
                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning(2, "Account locked");
                        return View("Lockout");
                    }
                    if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError("Email isn't verified");
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError("Wrong password.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("Account does not exist.");
                    return View(model);
                }         
            }
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost("/logout/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("User logoff");
            HttpContext.Session.Clear();
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Index", "Home", new {area = ""});
        }
        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new AppUserModel
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    LoginType = UserEnumData.LOGIN_WEBSITE,
                    Avatar = UserEnumData.IMAGE_DEFAULT,
                    CustomerSegment = 0,
                    Status = 1
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Create user successful");

                    try
                    {
                        await _userManager.AddToRoleAsync(user, RoleName.Customer);

                        //await SendPromotionToNewCustomer(user);

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {

                            return RedirectToAction("SendOtp", "Account", new { email = user.Email, typeService = DShopConst.OTP_CONFIRM_EMAIL });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(ex.Message);
                    }              
                }

                ModelState.AddModelError(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task SendPromotionToNewCustomer(AppUserModel user)
        {
            var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.CategoryCouponName == DShopConst.NEW_CUSTOMER);
            if (promotion == null)
            {
                promotion = new PromotionModel { CategoryCouponName = DShopConst.NEW_CUSTOMER };
                await _context.Promotions.AddAsync(promotion);
                await _context.SaveChangesAsync();
            }
            CouponModel couponModel = new CouponModel
            {
                CouponName = "Promotion for new customer",
                CouponCode = "NEWCUSTOMER_" + user.UserName.ToUpper(),
                Value = 50000,
                DateStart = DateTime.Today,
                DateExpired = DateTime.Today.AddDays(7),
                Quantity = 1,
                Status = 1,
                Description = "Free shipping for new customers' first order",
                PromotionId = promotion.Id
            };

            try
            {
                await _context.Coupons.AddAsync(couponModel);
                await _context.SaveChangesAsync();

                var infoShop = await _context.InformationShops.FirstOrDefaultAsync();
                await _emailSender.SendEmailCouponForNewCustomer(user, couponModel, infoShop);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message);
            }

        }
        
        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterConfirmation(string userId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return NotFound();
            }
           
            return View(user);
        }



        // GET: /Account/ConfirmEmailByCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmailByOTP(string userId, string codeConfirmEmail)
        {
            if (userId == null || codeConfirmEmail == null)
            {
                return View("ErrorConfirmEmail");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("ErrorConfirmEmail");
            }

            var otpConfirm = Request.Cookies[DShopConst.OTP_CONFIRM_EMAIL + user.UserName];
            if (codeConfirmEmail.Equals(otpConfirm))
            {
                
                user.EmailConfirmed = true;
                _context.Update(user);
                await _context.SaveChangesAsync();
                await SendPromotionToNewCustomer(user);
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Confirm email successful";
                return View();
            }
            TempData[DShopConst.TEMPDATA_ERROR] = "Code is invalid";
            return RedirectToAction("RegisterConfirmation", "Account", new { userId = user.Id });
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("ErrorConfirmEmail");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("ErrorConfirmEmail");
            }
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "ErrorConfirmEmail");
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error using external service: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            string externalEmail = null;
            AppUserModel externalEmailUser = null;
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                externalEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
            }

            if (externalEmail != null)
            {
                externalEmailUser = await _userManager.FindByEmailAsync(externalEmail);
            }
            if(externalEmailUser != null && externalEmailUser.Status == 0)
            {
                ModelState.AddModelError("Account was deleted ");
                return View(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // update token
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        } 

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                // Input.Email
                var registeredUser = await _userManager.FindByEmailAsync(model.Email);
                string externalEmail = null;
                AppUserModel externalEmailUser = null;
                
                // Claim 
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    externalEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                }

                if (externalEmail != null)
                {
                    externalEmailUser = await _userManager.FindByEmailAsync(externalEmail);
                }

                if ((registeredUser != null) && (externalEmailUser != null))
                {
                    // externalEmail  == Input.Email
                    if (registeredUser.Id == externalEmailUser.Id)
                    {
                        // Link account, login
                        var resultLink = await _userManager.AddLoginAsync(registeredUser, info);
                        if (resultLink.Succeeded)
                        {
                            await _signInManager.SignInAsync(registeredUser, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    else 
                    {
                        // registeredUser = externalEmailUser (externalEmail != Input.Email)
                        /*
                            info => user1 (mail1@abc.com)
                                 => user2 (mail2@abc.com)
                        */
                        ModelState.AddModelError(string.Empty, "Account cannot be linked, please use another email");
                        return View();
                    }
                }


                if ((externalEmailUser != null) && (registeredUser == null))
                {
                    ModelState.AddModelError(string.Empty, "No support for creating new accounts - have different email from the external service");
                    return View();                    
                }

                if((externalEmailUser == null) && (externalEmail == model.Email)) 
                {
                    var userName = externalEmail.Split('@');

                    var newUser = new AppUserModel() {
                        UserName = userName[0],
                        Email = externalEmail,
                        LoginType = UserEnumData.LOGIN_GMAIL,
                        Avatar = UserEnumData.IMAGE_DEFAULT,
                        CustomerSegment = 0,
                        Status = 1
                    };

                    var resultNewUser = await _userManager.CreateAsync(newUser);
                    if (resultNewUser.Succeeded)
                    {
                        await SendPromotionToNewCustomer(newUser);

                        await _userManager.AddToRoleAsync(newUser, RoleName.Customer);

                        await _userManager.AddLoginAsync(newUser, info);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                        await _userManager.ConfirmEmailAsync(newUser, code);

                        await _signInManager.SignInAsync(newUser, isPersistent: false);

                        return LocalRedirect(returnUrl);

                    }
                    else
                    {
                        ModelState.AddModelError("Cannot create new account");
                        return View();   
                    }
                }           


                var user = new AppUserModel { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);

                        // Update any authentication tokens as well
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                        return LocalRedirect(returnUrl);
                    }
                }
                ModelState.AddModelError(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user.LoginType == UserEnumData.LOGIN_GMAIL)
                {
                    return View("NoNeedPassword");
                }

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                return RedirectToAction("SendOtp", "Account" ,new { email = user.Email, typeService = DShopConst.OTP_RESET_PASSWORD });
   
            }
            return View(model);
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtpConfirmEmailAgian(string? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return RedirectToAction("SendOtp", "Account", new { email = user.Email, typeService = DShopConst.OTP_CONFIRM_EMAIL });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtpResetPaswordAgain(string? userId)
        {
            if ( String.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(userId);
            if(user == null) 
            {
                return NotFound(); 
            }
            return RedirectToAction("SendOtp", "Account", new { email = user.Email, typeService = DShopConst.OTP_RESET_PASSWORD });
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp(string email, string typeService)
        {
            if ( String.IsNullOrEmpty(email))
            {
                return View("NotFoundEmail");
            }
            var user = await _userManager.FindByEmailAsync (email);
            if (user == null)
            {
                return View("NotFoundEmail");
            }

            var rand = new Random();
            int codeEmail = rand.Next(100000, 999999);
            var cookieOptionss = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(10),
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };

            var infoShop = await _context.InformationShops.FirstOrDefaultAsync();
            await _emailSender.SendEmailOTP(user, codeEmail.ToString(), typeService, infoShop);

            if (typeService == DShopConst.OTP_CONFIRM_EMAIL)
            {
                Response.Cookies.Append(DShopConst.OTP_CONFIRM_EMAIL + user.UserName, codeEmail.ToString(), cookieOptionss);
                return RedirectToAction("RegisterConfirmation", "Account", new { userId = user.Id });
            }
            if(typeService == DShopConst.OTP_RESET_PASSWORD)
            {
                Response.Cookies.Append(DShopConst.OTP_RESET_PASSWORD + user.UserName, codeEmail.ToString(), cookieOptionss);
                return RedirectToAction("ForgotPasswordConfirmation", "Account", new { email = user.Email });
            }           
         
            return View("NotFoundEmail");
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordConfirmation(string email)
        {
            if( String.IsNullOrEmpty(email))
            {
                return View("NotFoundEmail");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if(user == null) {
                return View("NotFoundEmail");
            }
            return View(user);
        }

       
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordConfirmation(string userId, string codeResetPasswordl)
        {
            if (userId == null || codeResetPasswordl == null)
            {
                return View("ErrorConfirmEmail");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("ErrorConfirmEmail");
            }

            var otpConfirm = Request.Cookies[DShopConst.OTP_RESET_PASSWORD + user.UserName];
            if (codeResetPasswordl.Equals(otpConfirm))
            {

                return RedirectToAction("ResetPassword", "Account", new {email = user.Email});
            }
            TempData[DShopConst.TEMPDATA_ERROR] = "Code is invalid";
            return RedirectToAction("ForgotPasswordConfirmation", "Account", new { email = user.Email });
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmEmailAgain()
        {
            return View();
        }


        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                return View("NotFoundEmail");
            }
            ViewBag.Email = email;
            return View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            ViewBag.Email = model.Email;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }

            await _userManager.RemovePasswordAsync(user);
            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Password);
            if (addPasswordResult.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            ModelState.AddModelError(addPasswordResult);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }        

        //
        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View(DShopConst.TEMPDATA_ERROR);
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }
        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View(DShopConst.TEMPDATA_ERROR);
            }
            // Dùng mã Authenticator
            if (model.SelectedProvider == "Authenticator")
            {
                return RedirectToAction(nameof(VerifyAuthenticatorCode), new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View(DShopConst.TEMPDATA_ERROR);
            }

            var message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await _emailSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

            return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }
        //
        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View(DShopConst.TEMPDATA_ERROR);
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        //
        // GET: /Account/VerifyAuthenticatorCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View(DShopConst.TEMPDATA_ERROR);
            }
            return View(new VerifyAuthenticatorCodeViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyAuthenticatorCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeViewModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Wrong code.");
                return View(model);
            }
        }
        //
        // GET: /Account/UseRecoveryCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UseRecoveryCode(string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View(DShopConst.TEMPDATA_ERROR);
            }
            return View(new UseRecoveryCodeViewModel { ReturnUrl = returnUrl });
        }

        //
        // POST: /Account/UseRecoveryCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Wrong recovery code.");
                return View(model);
            }
        }

        //[Route("/accessdebied.html")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }



    
  }
}
