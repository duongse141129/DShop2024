using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class InformationShopAndBranchesVM
    {
        public InformationShopModel InformationShop { get; set; }
        public List<BranchModel> Branches { get; set; }
    }
}
