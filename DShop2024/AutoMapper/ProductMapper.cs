using AutoMapper;
using DShop2024.Areas.Admin.Models.Product;
using DShop2024.Models;
using DShop2024.ViewModels;

namespace DShop2024.AutoMapper
{
    public class ProductMapper : Profile
    {

        public ProductMapper() 
        {

            CreateMap<CreateProductRequest, ProductModel>();
            CreateMap<UpdateProductRequest, ProductModel>()
                .ReverseMap();

            CreateMap<ProductModel, ProductViewModel>()
                .ForMember(pv => pv.BrandName, p => p.MapFrom(p => p.Brand.BrandName) )
                .ForMember(pv => pv.CategoryName, p => p.MapFrom(p => p.Category.CategoryName) )
                .ForMember(pv => pv.AveragePoint, p => p.MapFrom(p => p.Ratings.Any() ? p.Ratings.Where(r => r.ProductId == p.Id && r.Status != 0).Average(r => r.Star) : 0));

        }
    }
}
