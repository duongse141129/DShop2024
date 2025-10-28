using DShop2024.EnumData;
using DShop2024.Hubs;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
	public class ChatController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ChatController(DShopContext context, UserManager<AppUserModel> userManager, IHubContext<ChatHub> hubContext, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = Menu.Admin.Chat;

            var customers = await (from u in _context.Users
                                   join ur in _context.UserRoles on u.Id equals ur.UserId
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where r.Name == RoleName.Customer && u.Status != 0
                                   orderby u.Id descending
                                   select  new CustomerAndMessageViewModel { UserId = u.Id, UserName = u.UserName, Avatar = u.Avatar} ).ToListAsync();

            foreach ( var customer in customers )
            {
                var latestMessagesOfCustomer = await (from t in _context.Messages
                                                join tm in (
                                                    from t2 in _context.Messages
                                                    group t2 by t2.UserId into g
                                                    select new
                                                    {
                                                        UserId = g.Key,
                                                        date = g.Max(x => x.Timestamp)
                                                    }
                                                ) on new { t.UserId, t.Timestamp } equals new { UserId = tm.UserId, Timestamp = tm.date }
                                                where t.UserId == customer.UserId
                                                select new
                                                {
                                                    t.Id,
                                                    t.ContentMessage,
                                                    t.Timestamp,
                                                    t.IsRead
                                                }).FirstOrDefaultAsync();

                if (latestMessagesOfCustomer != null)
                {
                    customer.LatestMessage = latestMessagesOfCustomer.ContentMessage ?? "";
                    customer.Timestamp = latestMessagesOfCustomer.Timestamp.ToString();
                    customer.IsRead = latestMessagesOfCustomer.IsRead ?? false;
                }
            }
            return View(customers.OrderByDescending(c => c.Timestamp));
        }
        public async Task<IActionResult> SearchUserName(string userName)
        {
            ViewBag.sidebar = Menu.Admin.Chat;
            ViewBag.searchUserName = userName;
            if (String.IsNullOrEmpty(userName))
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Username does not exist";
                return RedirectToAction("Index");
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserName == userName && m.Status != 0);
            if (user == null)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Username does not exist";
                return RedirectToAction("Index");
            }
            return RedirectToAction("ChatWithCustomer", new { customerId  = user.Id});
        }

        public async Task<IActionResult> ChatWithCustomer(string customerId)
        {
            ViewBag.sidebar = Menu.Admin.Chat;
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
            await ReadMessage(customerId);
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
                                                         Receiver = m.ReceiverId,
                                                         Avatar = u.Avatar
                                                     })
                                                     .ToListAsync();

            var reciver = await _userManager.FindByIdAsync(customerId);
            ViewBag.receiver = customerId;
            ViewBag.receiverName = reciver.UserName;
            return View(messages);
        }

        public async Task ReadMessage(string customerId)
        {
            try
            {
                var latestMessages = _context.Messages
                      .GroupBy(m => m.UserId)
                      .Select(g => new { UserId = g.Key, Date = g.Max(m => m.Timestamp) });

                var result = await _context.Messages
                    .Join(latestMessages, m => new { m.UserId, m.Timestamp }, lm => new { lm.UserId, Timestamp = lm.Date }, (m, lm) => m)
                    .Where(m => m.UserId == customerId)
                    .Select(m => m.Id).ToListAsync();

                foreach (var messageId in result)
                {
                    MessageModel m = await _context.Messages.FindAsync(messageId);
                    m.IsRead = true;
                    _context.Messages.Update(m);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Read Message fail " +ex.Message;
            }

        }

        public async Task<IActionResult> ReadAllMessage2days(string customerId)
        {
            try
            {
                var messageIds = await _context.Messages
                        .Join(_context.Users, m => m.UserId, u => u.Id, (m, u) => new { m, u })
                        .Join(_context.UserRoles, mu => mu.u.Id, ur => ur.UserId, (mu, ur) => new { mu.m, mu.u, ur })
                        .Join(_context.Roles, mur => mur.ur.RoleId, r => r.Id, (mur, r) => new { mur.m, mur.u, mur.ur, r })
                        .Where(mur => mur.u.Status != 0 && mur.r.Name == RoleName.Customer && mur.m.Timestamp > DateTime.Now.AddDays(-2))
                        .Select(mur => mur.m.Id).ToListAsync();

                foreach (var id in messageIds)
                {
                    MessageModel m = await _context.Messages.FindAsync(id);
                    m.IsRead = true;
                    _context.Messages.Update(m);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { success = true, Message = "read all message successful" });
            }
            catch (Exception ex)
            {

                return Ok(new { success = true, Message = "read all message fail "+ ex.Message });
            }

        }


        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string messageInput)
        {
            ViewBag.sidebar = Menu.Admin.Chat;
            var user = await _userManager.GetUserAsync(this.User);
            if (!String.IsNullOrEmpty(messageInput))
            {
                MessageModel model = new MessageModel
                {
                    ContentMessage = messageInput,
                    Timestamp = DateTime.Now,
                    UserId = user.Id,
                    ReceiverId = receiverId,
                    IsRead = false,
                    IsImage = false
                };
                await _context.Messages.AddAsync(model);
                await _context.SaveChangesAsync();
                var role = await _userManager.GetRolesAsync(user);

                var receiver = await _userManager.FindByIdAsync(receiverId);

                MessageViewModel modelVM = new MessageViewModel
                {
                    ContentMessage = messageInput,
                    Timestamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                    UserName = user.UserName,
                    RoleName = role.FirstOrDefault(),
                    Receiver = receiver.UserName,
                    Avatar = user.Avatar,
                    IsImage = false,
                    PathImage = $"/media/avatar/{user.Avatar}"
                };

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.UserName, modelVM);
                return Ok(new { success = true, Message = "Send message successful" });

            }
            TempData[DShopConst.TEMPDATA_ERROR] = "Messages are empty";
            return Ok(new { success = false, Message = "Send message fail" });
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string receiverId)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                var role = await _userManager.GetRolesAsync(user);
                var receiver = await _userManager.FindByIdAsync(receiverId);
                var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, $"media/message/{receiver}");
                var filePath = Path.Combine(folderPath, fileName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                string htmlImage = string.Format(
                    "<img src=\"/media/message/{0}/{1}\" class=\"post-image\">", receiver.UserName, fileName);

                MessageModel model = new MessageModel
                {
                    ContentMessage = htmlImage,
                    Timestamp = DateTime.Now,
                    UserId = user.Id,
                    ReceiverId = receiverId,
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
                    RoleName = role.FirstOrDefault(),
                    Receiver = receiver.UserName,
                    Avatar = user.Avatar,
                    IsImage = true,
                    PathImage = $"/media/avatar/{user.Avatar}",
                };

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.UserName, modelVM);
                return Ok(new { success = true, Message = "Send message successful" });
            }
            return Ok(new { success = false, Message = "Send message fail" });
        }


    }
}
