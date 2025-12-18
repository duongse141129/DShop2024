using DShop2024.Models;

namespace DShop2024.Areas.Admin.Models.Order
{
    public class OrderWithOrderAndReturnDetailsVM
    { 
        public OrderModel Order { get; set; }
        public ReturnModel Return { get; set; }
        public List<OrderAndReturnDetailVM> Details { get; set; } = new List<OrderAndReturnDetailVM>();

        public decimal GrandTotal { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RemainingAmount { get; set; }

    }
}
