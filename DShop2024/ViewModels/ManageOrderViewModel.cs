using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class ManageOrderViewModel
    {
        public int CountCancelOrder { get; set; } = 0;
        public int CountNewOrder { get; set; } = 0;
        public int CountAcceptedOrder { get; set; } = 0;
        public int CountDeliveryOrder { get; set; } = 0;
        public int CountCompletedOrder { get; set; } = 0;
        public int CountTotalOrder { get; set; } = 0;
        public string SearchOrderCode { get; set; } = "";
        public List<OrderViewModel> ListOrder { get; set; } = new List<OrderViewModel>();
        public PagingModel Paging { get; set; }
    }
}
