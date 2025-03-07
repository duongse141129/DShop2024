using DShop2024.Hubs;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(DShopContext context, UserManager<AppUserModel> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var userWithRoles = await (from u in _context.Users
                                       join ur in _context.UserRoles on u.Id equals ur.UserId
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       where r.Name == "CUSTOMER"
                                       select new { User = u, RoleName = r.Name }).ToListAsync();

            return View(userWithRoles);
        }
        public async Task<IActionResult> ChatWithCustomer(string customerId)
        {
            List<MessageViewModel> messages = await (from m in _context.Messages
                                                     join u in _context.Users on m.UserId equals u.Id
                                                     join ur in _context.UserRoles on u.Id equals ur.UserId
                                                     join r in _context.Roles on ur.RoleId equals r.Id
                                                     where m.UserId == customerId || m.Receiver == customerId
                                                     select new MessageViewModel{ 
                                                         UserName = u.UserName, 
                                                         RoleName = r.Name, 
                                                         ContentMessage = m.ContentMessage,
                                                         Timestamp = m.Timestamp.ToString(),
                                                     })
                                                     .OrderBy(m => m.Timestamp)
                                                     .ToListAsync();
            ViewBag.receiver = customerId;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiver, string messageInput)
        {
            var user = await _userManager.GetUserAsync(this.User);
            //var messageInput = Request.Form["messageInput"];
            if (!String.IsNullOrEmpty(messageInput))
            {
                MessageModel model = new MessageModel
                {
                    ContentMessage = messageInput,
                    Timestamp = DateTime.Now,
                    UserId = user.Id,
                    Receiver = receiver
                };
                await _context.Messages.AddAsync(model);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "DShop2024", model.ContentMessage);
                return RedirectToAction("ChatWithCustomer", new { customerId =receiver});
            }
            TempData["error"] = "Messages are empty";
            return RedirectToAction("ChatWithCustomer");

        }
    }
}
