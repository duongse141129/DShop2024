namespace DShop2024.Areas.Identity.Models.Account
{
    public class OtpSession
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public int FailedAttempts { get; set; }
    }
}
