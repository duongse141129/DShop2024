using System.ComponentModel.DataAnnotations;

namespace DShop2024.ViewModels
{
    public class OrderViewModel
    {
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PaymentMethod { get; set; }
        public int Status { get; set; }
        public decimal TotalPrice { get; set; }
        public string AddressDelivery { get; set; }
        public string PhoneDelivery { get; set; }
        public string Consignee { get; set; }

        public decimal ShippingCost { get; set; }
        public decimal ValueCoupon { get; set; }
    }
}
