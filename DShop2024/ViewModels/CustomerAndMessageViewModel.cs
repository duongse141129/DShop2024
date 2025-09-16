namespace DShop2024.ViewModels
{
    public class CustomerAndMessageViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string LatestMessage { get; set; } = string.Empty;
        public string Timestamp { get; set; }   
        public bool IsRead { get; set; } = false;
    }
}
