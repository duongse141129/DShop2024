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
                               .ForMember(dest => dest.MainImage, act => act.Ignore())
                                .ReverseMap();

            CreateMap<ProductModel, ProductViewModel>()
                .ForMember(pv => pv.BrandName, p => p.MapFrom(p => p.Brand.BrandName) )
                .ForMember(pv => pv.CategoryName, p => p.MapFrom(p => p.Category.CategoryName))
                .ForMember(pv => pv.AveragePoint, p => p.MapFrom(p => p.Ratings.Any() ? p.Ratings.Where(r => r.ProductId == p.Id && r.Status != 0).Average(r => r.Star) : 0))
                .ForMember(dest => dest.SalePrice, opt => opt.MapFrom(src => src.Sales.Where(s => s.Status != 0 
                                                                                                && DateTime.Now >= s.SaleStartDate 
                                                                                                && DateTime.Now <= s.SaleEndDate)
                                                                                     .OrderByDescending(s => s.SaleStartDate)
                                                                                     .Select(s => (decimal?)s.SalePrice)
                                                                                     .FirstOrDefault()))
                .ForMember(dest => dest.IsOnSale, opt => opt.MapFrom(src =>src.Sales.Any(s => s.Status != 0 
                                                                                && DateTime.Now >= s.SaleStartDate 
                                                                                && DateTime.Now <= s.SaleEndDate))); 

        }
    }
}
