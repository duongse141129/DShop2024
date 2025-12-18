using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class ReturnViewModel 
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }

        public string CustomerName { get; set; }
        public string CustomerAvatar { get; set; }

        public string UpdateByName { get; set; }
        public string UpdateByAvatar { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsFullReturn { get; set; }

        public List<string> Images { get; set; } = new();
        public List<ReturnDetailViewModel> ReturnDetails { get; set; }
    }
}
