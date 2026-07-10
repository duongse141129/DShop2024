using AutoMapper;
using DShop2024.Areas.Admin.Models.Sale;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;

namespace DShop2024.AutoMapper
{
    public class SaleMapper : Profile
    {
        public SaleMapper()
        {
            CreateMap<SaleModel, EditSaleRequest>();

            CreateMap<SaleModel, SaleViewModel>()
                .ForMember(pv => pv.Status, p => p.MapFrom(p => GetStatusSale(p.SaleStartDate, p.SaleEndDate)));



        }
        private static string GetStatusSale(DateTime startDate, DateTime endDate)
        {
            var now = DateTime.Now;

            if (endDate < now)
                return SaleEnumData.StatusSale.Expired.ToString();

            if (startDate > now)
                return SaleEnumData.StatusSale.NotYet.ToString();

            return SaleEnumData.StatusSale.Active.ToString();
        }
    }
}
