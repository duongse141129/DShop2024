using DShop2024.EnumData;
using DShop2024.Hubs;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
	public class ChatController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly string sidebar = "chat";

        public ChatController(DShopContext context, UserManager<AppUserModel> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = sidebar;
            var userWithRoles = await (from u in _context.Users
                                       join ur in _context.UserRoles on u.Id equals ur.UserId
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       where r.Name == RoleName.Customer
                                       select new { User = u, RoleName = r.Name }).ToListAsync();

            return View(userWithRoles);
        }

        public async Task<IActionResult> ChatWithCustomer(string customerId)
        {
            ViewBag.sidebar = sidebar;
            if (String.IsNullOrEmpty(customerId))
            {
                return NotFound();
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == customerId && m.Status != 0);
            if (user == null)
            {
                return NotFound();
            }

            List<MessageViewModel> messages = await (from m in _context.Messages
                                                     join u in _context.Users on m.UserId equals u.Id
                                                     join ur in _context.UserRoles on u.Id equals ur.UserId
                                                     join r in _context.Roles on ur.RoleId equals r.Id
                                                     where m.UserId == customerId || m.ReceiverId == customerId
                                                     orderby m.Timestamp
                                                     select new MessageViewModel{ 
                                                         UserName = u.UserName, 
                                                         RoleName = r.Name, 
                                                         ContentMessage = m.ContentMessage,
                                                         Timestamp = m.Timestamp.ToString("MM/dd/yyyy HH:mm:ss"),
                                                     })
                                                     .ToListAsync();

            var reciver = await _userManager.FindByIdAsync(customerId);
            ViewBag.receiver = customerId;
            ViewBag.receiverName = reciver.UserName;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiver, string messageInput)
        {
            ViewBag.sidebar = sidebar;
            var user = await _userManager.GetUserAsync(this.User);
            if (!String.IsNullOrEmpty(messageInput))
            {
                MessageModel model = new MessageModel
                {
                    ContentMessage = messageInput,
                    Timestamp = DateTime.Now,
                    UserId = user.Id,
                    ReceiverId = receiver
                };
                await _context.Messages.AddAsync(model);
                await _context.SaveChangesAsync();
                var role = await _userManager.GetRolesAsync(user);

                var reciver = await _userManager.FindByIdAsync(receiver);
                MessageViewModel modelVM = new MessageViewModel
                {
                    ContentMessage = messageInput,
                    Timestamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                    UserName = user.UserName,
                    RoleName = role.FirstOrDefault(),
                    Receiver = reciver.UserName
                };

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.UserName, modelVM);
                return Ok(new { success = true, Message = "Send message successful" });

            }
            TempData[DShopConst.TEMPDATA_ERROR] = "Messages are empty";
            return Ok(new { success = false, Message = "Send message fail" });
        }
    }
}
