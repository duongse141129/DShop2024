using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Tls.Crypto;
using Microsoft.EntityFrameworkCore;
using DShop2024.EnumData;

namespace DShop2024.Repository.Components
{
    public class NotificationMessageViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public NotificationMessageViewComponent(DShopContext context)
        {
            _context = context;
        }

        public static string GetDayLeft(DateTime dateTime)
        {
            var d = DateTime.Today.Date - dateTime.Date;
            if(d.Days < 1)
            {
                return "Today "+ dateTime.ToString("hh:mm tt");
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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //var subquery = _context.Messages
            //    .GroupBy(m => m.UserId)
            //    .Select(g => new
            //    {
            //        UserId = g.Key,
            //        MaxTimestamp = g.Max(m => m.Timestamp)
            //    });

            //var query2 = _context.Messages
            //    .Join(subquery,
            //        m => new { m.UserId, m.Timestamp },
            //        tm => new { tm.UserId, Timestamp = tm.MaxTimestamp },
            //        (m, tm) => m)
            //    .Join(_context.Users,
            //        m => m.UserId,
            //        u => u.Id,
            //        (m, u) => new { Message = m, User = u })
            //    .Join(_context.UserRoles,
            //        x => x.User.Id,
            //        ur => ur.UserId,
            //        (x, ur) => new { x.Message, x.User, UserRole = ur })
            //    .Join(_context.Roles,
            //        x => x.UserRole.RoleId,
            //        r => r.Id,
            //        (x, r) => new { x.Message, x.User, Role = r })
            //    .Where(x => x.User.Status != 0 && x.Role.Name == "CUSTOMER")
            //    .GroupBy(x => new { x.User.UserName, x.Message.ContentMessage })
            //    .Select(g => new
            //    {
            //        UserName = g.Key.UserName,
            //        ContentMessage = g.Key.ContentMessage,
            //        MaxTimestamp = g.Max(x => x.Message.Timestamp)
            //    })
            //    .OrderByDescending(x => x.MaxTimestamp)
            //    .ToList();


            var latestMessagePerUser2daysAgo = await ( from m in _context.Messages
                        join u in _context.Users on m.UserId equals u.Id
                        join ur in _context.UserRoles on u.Id equals ur.UserId
                        join r in _context.Roles on ur.RoleId equals r.Id
                        join tm in (
                            from t in _context.Messages
                            group t by t.UserId into g
                            select new
                            {
                                UserId = g.Key,
                                date = g.Max(x => x.Timestamp)
                            }
                        ) on new { m.UserId, m.Timestamp } equals new { tm.UserId, Timestamp = tm.date }
                        where u.Status != 0 && r.Name == RoleName.Customer
                        group new { u, m } by new { u.Id, u.Avatar, u.UserName, m.ContentMessage, m.IsRead} into g
                        where g.Max(x => x.m.Timestamp) > DateTime.Today.AddDays(-2)
                        orderby g.Min(x => x.m.Timestamp) descending
                        select new MessageViewModel
                        {
                            UserId = g.Key.Id,
                            Avatar = g.Key.Avatar,
                            UserName = g.Key.UserName,
                            ContentMessage = g.Key.ContentMessage,
                            Timestamp =  GetDayLeft(g.Max(x => x.m.Timestamp)),
                            IsRead = g.Key.IsRead
                        }).ToListAsync() ;
  
            return View(latestMessagePerUser2daysAgo);

        }
    }
}
