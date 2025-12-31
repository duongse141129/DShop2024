using DShop2024.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DShop2024.ViewModels
{
    public class ShopProductViewModel
    {
        public List<ProductViewModel> Products { get; set; }
        public ProductFilter Filter { get; set; }
        public PagingModel Paging { get; set; }
        public SelectList SortByList { get; set; }

    }
}
