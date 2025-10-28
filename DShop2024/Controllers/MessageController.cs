using DShop2024.EnumData;
using DShop2024.Hubs;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    public class MessageController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _environment;

        public MessageController(DShopContext context, UserManager<AppUserModel> userManager, IHubContext<ChatHub> hubContext, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _environment = environment;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = Menu.Home.Chat;
            var user = await _userManager.GetUserAsync(this.User);

            List<MessageViewModel> messages = await (from m in _context.Messages
                                                     join u in _context.Users on m.UserId equals u.Id
                                                     join ur in _context.UserRoles on u.Id equals ur.UserId
                                                     join r in _context.Roles on ur.RoleId equals r.Id
                                                     where m.UserId == user.Id || m.ReceiverId == user.Id
                                                     orderby m.Timestamp 
                                                     select new MessageViewModel
                                                     {
                                                         UserName = u.UserName,
                                                         RoleName = r.Name,
                                                         ContentMessage = m.ContentMessage,
                                                         Timestamp = m.Timestamp.ToString("MM/dd/yyyy HH:mm:ss"),
                                                         Avatar = u.Avatar
                                                     })
                                         .ToListAsync();
            return View(messages);
		}


        [HttpPost]
        public async Task<IActionResult> SendMessage(string messageInput)
        {
            var user = await _userManager.GetUserAsync(this.User);
            if (!String.IsNullOrEmpty(messageInput))
            {
                MessageModel model = new MessageModel
                {
                    ContentMessage = messageInput,
                    Timestamp = DateTime.Now,
                    UserId = user.Id,
                    IsRead = false,
                    IsImage = false
                };
                await _context.Messages.AddAsync(model);
                await _context.SaveChangesAsync();

                MessageViewModel modelVM = new MessageViewModel
                {
                    ContentMessage = messageInput,
                    Timestamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                    UserName = user.UserName,
                    UserId = user.Id,
                    RoleName = RoleName.Customer,
                    Avatar = user.Avatar,
                    IsImage = false,
                    PathImage = $"/media/avatar/{user.Avatar}",
                    PathUser = $"/Admin/Chat/ChatWithCustomer?customerId={user.Id}",
                    DaysLeftTime = GetDayLeft(DateTime.Now)

                };

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.UserName, modelVM);
                return Ok(new { success = true, Message = "Send message successful" });
                
            }
            return Ok(new { success = false, Message = "Send message fail" });
        }


        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                var fileName = Guid.NewGuid().ToString() +  "_" + Path.GetFileName(file.FileName);
                var folderPath = Path.Combine(_environment.WebRootPath, $"media/message/{user.UserName}");
                var filePath = Path.Combine(folderPath, fileName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                string htmlImage = string.Format(
                    "<img src=\"/media/message/{0}/{1}\" class=\"post-image\">", user.UserName, fileName);

                MessageModel model = new MessageModel
                {
                    ContentMessage = htmlImage,
                    Timestamp = DateTime.Now,
                    UserId = user.Id,
                    IsRead = false,
                    IsImage = true
                };
                await _context.Messages.AddAsync(model);
                await _context.SaveChangesAsync();

                MessageViewModel modelVM = new MessageViewModel
                {
                    ContentMessage = htmlImage,
                    Timestamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                    UserName = user.UserName,
                    UserId = user.Id,
                    RoleName = RoleName.Customer,
                    Avatar = user.Avatar,
                    IsImage = true,
                    PathImage = $"/media/avatar/{user.Avatar}",
                    PathUser = $"/Admin/Chat/ChatWithCustomer?customerId={user.Id}",
                    DaysLeftTime = GetDayLeft(DateTime.Now)

                };
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.UserName, modelVM);
                return Ok(new { success = true, Message = "Send message successful" });

            }
            return Ok(new { success = false, Message = "Send message fail" });
        }



        private static string GetDayLeft(DateTime dateTime)
        {
            var d = DateTime.Today.Date - dateTime.Date;
            if (d.Days < 1)
            {
                return "Today " + dateTime.ToString("hh:mm tt");
            }
            if (d.Days == 1)
            {
                return "1 day ago " + dateTime.ToString("hh:mm tt");
            }
            if (d.Days == 2)
            {
                return "2 days ago " + dateTime.ToString("hh:mm tt");
            }
            return "";
        }

    }
}
