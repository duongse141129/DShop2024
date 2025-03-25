using AutoMapper;
using DShop2024.Areas.Admin.Models.Product;
using DShop2024.Models;

namespace DShop2024.AutoMapper
{
    public class ProductMapper : Profile
    {
        public ProductMapper() 
        {
            CreateMap<CreateProductRequest, ProductModel>();
            CreateMap<UpdateProductRequest, ProductModel>()
                .ForMember(dest => dest.Image, act => act.Ignore())
                .ForMember(dest => dest.ImageUpload, act => act.Ignore())
                .ForMember(dest => dest.Status, act => act.Ignore())
                .ReverseMap();
        }
    }
}
