using DShop2024.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.ViewModels
{
    public class MessageViewModel
    {
        public string ContentMessage { get; set; }
        public String Timestamp { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string RoleName { get; set; }
        public string Receiver { get; set; }
        public string PathImage { get; set; }
    }
}
