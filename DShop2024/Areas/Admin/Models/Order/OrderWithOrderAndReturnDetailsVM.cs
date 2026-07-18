using DShop2024.Models;
using DShop2024.ViewModels;

namespace DShop2024.Areas.Admin.Models.Order
{
    public class OrderWithOrderAndReturnDetailsVM   
    { 
        public OrderViewModel Order { get; set; }
        public ReturnViewModel Return { get; set; }
        public List<OrderAndReturnDetailVM> Details { get; set; } = new List<OrderAndReturnDetailVM>();

        public decimal GrandTotal { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RemainingAmount { get; set; }

    }
}
