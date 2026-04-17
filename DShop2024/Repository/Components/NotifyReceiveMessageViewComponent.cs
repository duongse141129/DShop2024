using DShop2024.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class NotifyReceiveMessageViewComponent : ViewComponent
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;

        public NotifyReceiveMessageViewComponent(DShopContext context, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            bool receive = true;
            if (_signInManager.IsSignedIn(HttpContext.User))
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    return View(receive);
                }
                receive = await _context.Messages.Where(r => r.ReceiverId == user.Id)
                                        .OrderByDescending(s => s.Timestamp)
                                        .Select(i => i.IsRead)
                                        .FirstOrDefaultAsync() ?? true;
            }

            return View(receive);

        }
    }
}
