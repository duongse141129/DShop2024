using DShop2024.Hubs;
using DShop2024.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
    public class MessageController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(DShopContext context, UserManager<AppUserModel> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(this.User);
            List<MessageModel> messages = await _context.Messages.
                                        Where(u => u.UserId == user.Id || u.Receiver == user.Id)
                                        .OrderBy(p => p.Timestamp)
                                        .ToListAsync();
			return View(messages);
		}

        [HttpPost]
        public async Task<IActionResult> SendMessage( string messageInput)
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
                    Receiver = "DShop2024"
                };
                await _context.Messages.AddAsync(model);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.UserName, model.ContentMessage);
                return RedirectToAction("Index");
            }
            TempData["error"] = "Messages are empty";
            return RedirectToAction("Index");

        }
    }
}
