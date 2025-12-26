using AutoMapper;
using DShop2024.Models;
using DShop2024.ViewModels;

namespace DShop2024.AutoMapper
{
    public class ReturnMapper : Profile
    {
        public ReturnMapper()
        {
            CreateMap<ReturnDetailModel, ReturnDetailViewModel>()
                .ForMember(pv => pv.ProductName, p => p.MapFrom(p => p.Product.ProductName))
                .ForMember(pv => pv.ProductImage, p => p.MapFrom(p => p.Product.MainImage))
                .ForMember(pv => pv.Images, p => p.MapFrom(p => GetListImages(p.Images)));

            CreateMap<ReturnModel, ReturnViewModel>()
                .ForMember(pv => pv.CustomerAvatar, p => p.MapFrom(p => p.Customer.Avatar))
                .ForMember(pv => pv.CustomerName, p => p.MapFrom(p => p.Customer.UserName))
                .ForMember(pv => pv.UpdateByName, p => p.MapFrom(p => p.UpdateBy.UserName))
                .ForMember(pv => pv.UpdateByAvatar, p => p.MapFrom(p => p.UpdateBy.Avatar))
                .ForMember(pv => pv.OrderCode, p => p.MapFrom(p => p.Order.OrderCode))
                .ForMember(pv => pv.Images, p => p.MapFrom(p => GetListImages(p.Images)))
                .ForMember(pv => pv.IsFullReturn, p => p.MapFrom(p => CheckIsFullReturn(p.Reason)));
        }
        private static bool CheckIsFullReturn(string reason)
        {
            if (!String.IsNullOrEmpty(reason)) return true;
            return false;
        }
        private static List<string> GetListImages(string pathImage)
        {
            List<string> result = new List<string>();
            if (String.IsNullOrEmpty(pathImage))
            {
                return result;
            }
            var listImg = pathImage.Split("|");
            if(listImg.Count() > 0)
            {
                foreach (var img in listImg)
                {
                    result.Add(img);
                }
            }
            return result;
        }

    }

}
