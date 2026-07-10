using AutoMapper;
using DShop2024.Models;
using DShop2024.ViewModels;

namespace DShop2024.AutoMapper
{
    public class OrderMapper : Profile
    {
        public OrderMapper()
        {
            CreateMap<OrderDetailModel, ReturnDetailModel>()
                .ForMember(pv => pv.ProductId, p => p.MapFrom(p => p.ProductId))
                .ForMember(pv => pv.Quantity, p => p.MapFrom(p => p.Quantity))
                .ForMember(pv => pv.OrderDetailId, p => p.MapFrom(p => p.Id))
                .ForMember(pv => pv.OriginalPricePerUnit, p => p.MapFrom(p => p.OriginalPrice))
                .ForMember(pv => pv.PricePerUnit, p => p.MapFrom(p => p.Price));

            CreateMap<OrderDetailModel, CreateReturnDetailRequest>()
                    .ForMember(pv => pv.ProductId, p => p.MapFrom(p => p.ProductId))
                    .ForMember(pv => pv.Quantity, p => p.MapFrom(p => p.Quantity))
                    .ForMember(pv => pv.OrderDetailId, p => p.MapFrom(p => p.Id))
                    .ForMember(pv => pv.OriginalPricePerUnit, p => p.MapFrom(p => p.OriginalPrice))
                    .ForMember(pv => pv.PricePerUnit, p => p.MapFrom(p => p.Price));

            CreateMap<OrderDetailModel, OrderDetailViewModel>()
                .ForMember(pv => pv.ProductName, p => p.MapFrom(p => p.Product.ProductName))
                .ForMember(pv => pv.MainImage, p => p.MapFrom(p => p.Product.MainImage));

            CreateMap<OrderModel, OrderViewModel>()
                    .ForMember(pv => pv.CustomerAvatar, p => p.MapFrom(p => p.User.Avatar))
                    .ForMember(pv => pv.CustomerName, p => p.MapFrom(p => p.User.UserName))
                    .ForMember(pv => pv.CustomerEmail, p => p.MapFrom(p => p.User.Email))
                    .ForMember(pv => pv.CustomerId, p => p.MapFrom(p => p.User.Id))
                    .ForMember(pv => pv.UpdateByName, p => p.MapFrom(p => p.UpdateBy.UserName))
                    .ForMember(pv => pv.UpdateByAvatar, p => p.MapFrom(p => p.UpdateBy.Avatar))
                    .ForMember(pv => pv.PaymentName, p => p.MapFrom(p => p.PaymentMethod.PaymentName))
                    .ForMember(pv => pv.TotalQuantity, p => p.MapFrom(p => p.OrderDetails.Select(s => s.Quantity).Sum() ))
                    .ForMember(pv => pv.AllowReturn, p => p.MapFrom(p => CheckAllowReturn(p.DateUpdate, p.Status)))
                    .ForMember(pv => pv.Return, p => p.MapFrom(p => p.Returns.FirstOrDefault()))
                    .ForMember(pv => pv.IsReturn, p => p.MapFrom(p => p.Returns.Count > 0 ? true : false))
                    .ForMember(pv => pv.IsFullReturn, p => p.MapFrom(p => CheckIsFullReturn(p.Returns)))
                    .ForMember(pv => pv.IsReturnRejected, p => p.MapFrom(p => p.Returns.FirstOrDefault().Status == 0 ? true : false));


        }
        private static bool CheckIsFullReturn(ICollection<ReturnModel> Returns)
        {
            if(Returns?.Count == 0) return false;
            var returnModel = Returns.FirstOrDefault();
            if(!String.IsNullOrEmpty(returnModel.Reason)) return true;
            return false;
        }
        private static bool CheckAllowReturn(DateTime updateDate, int status)
        {
            if(status != 4) return false;
            if(updateDate.AddDays(7) >= DateTime.Now ) return true;
            return false;
        }

    }
}
