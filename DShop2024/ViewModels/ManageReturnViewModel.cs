using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class ManageReturnViewModel
    {
        public int CountRejectedReturned { get; set; } = 0;
        public int CountFullyReturned { get; set; } = 0;
        public int CountPartlyReturned { get; set; } = 0;
        public int CountQuantityReturned { get; set; } = 0;
        public string Search { get; set; } = "";
        public List<ReturnViewModel> ListReturn{ get; set; } = new List<ReturnViewModel>();
        public PagingModel Paging { get; set; }
    }
}
