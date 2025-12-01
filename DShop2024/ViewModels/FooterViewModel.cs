using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class FooterViewModel
    {
        public InformationShopModel informationShop { get; set; }   
        public List<string> payments { get; set; }  =new List<string>();
    }
}
