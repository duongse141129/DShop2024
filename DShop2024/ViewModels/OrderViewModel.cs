

using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public decimal GrandTotal { get; set; }
        public string AddressDelivery { get; set; }
        public string PhoneDelivery { get; set; }
        public string Consignee { get; set; }

        public decimal ShippingCost { get; set; }
        public decimal ValueCoupon { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAvatar { get; set; }
        public string CustomerEmail { get; set; }
        public string PaymentName { get; set; }
        public int PaymentStatus { get; set; }
        public bool AllowReturn { get; set; }
        public bool IsReturn { get; set; }
        public bool IsFullReturn { get; set; }
        public int TotalQuantity { get; set; }

        public string UpdateByName { get; set; }
        public string UpdateByAvatar { get; set; }
        public DateTime DateUpdate { get; set; }

        public List<OrderDetailViewModel> OrderDetails { get; set; }
        public List<OrderCouponsModel> OrderCoupons { get; set; }
        public ReturnViewModel Return { get; set; } 


    }
}
