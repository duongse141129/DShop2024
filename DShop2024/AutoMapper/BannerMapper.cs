using AutoMapper;
using DShop2024.Areas.Admin.Models.Banner;
using DShop2024.Areas.Admin.Models.Product;
using DShop2024.Models;

namespace DShop2024.AutoMapper
{
    public class BannerMapper : Profile
    {
        public BannerMapper()
        {
            CreateMap<CreateBannerRequest, BannerModel>();
        }
   
    }
}
