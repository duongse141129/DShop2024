using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class DataDashboardViewModel
    {
        public int CountProduct { get; set; } =0;
        public int CountUser { get; set; } = 0;
        public int CountOrder { get; set; } = 0;
        public int CountCancelOrder { get; set; } = 0;
        public int CountNewOrder { get; set; } = 0;
        public int CountAcceptedOrder { get; set; } = 0;
        public int CountDeliveryOrder { get; set; } = 0;
        public int CountCompletedOrder { get; set; } = 0;
        public int CountQuantityReturn { get; set; } = 0;
        public List<ProductViewModel> BestSaleProducts { get; set; } = new List<ProductViewModel>();
        public List<AppUserModel> ListCustomer { get; set; } = new List<AppUserModel>();
        public List<ContactModel> ListContact { get; set; } = new List<ContactModel>();
        public List<CouponViewModel> ListAvailableCouponVM { get; set; } = new List<CouponViewModel>();
    }
}
