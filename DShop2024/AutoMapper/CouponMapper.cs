using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;

namespace DShop2024.AutoMapper
{
    public class CouponMapper : Profile
    {
        public CouponMapper()
        {
            CreateMap<CouponModel, CouponViewModel>()
                .ForMember(d => d.DaysLeft, s => s.MapFrom(s => ConvertTimeToDuration(s.DateStart, s.DateExpired)))
                .ForMember(des => des.UsageCount, s => s.MapFrom(s => s.OrderCoupons.Where( c => c.Status == 1).Count()))
                .ForMember(des => des.IsShowing, s => s.MapFrom(s => CheckShowing(s.Status, s.Quantity,s.DateStart, s.DateExpired) ))
                .ForMember(des => des.AllowShow, s => s.MapFrom(s => AllowToShow(s.Status, s.Quantity, s.DateStart, s.DateExpired) ))
                .ForMember(des => des.CategoryCouponName, s => s.MapFrom(s => s.Promotion.CategoryCouponName));

            CreateMap<CouponModel, CouponItem>()
                .ForMember(d => d.ShortDescription, s => s.MapFrom(s => Summary(s.Value, s.Promotion.CategoryCouponName)))
                .ForMember(des => des.Promotion, s => s.MapFrom(s => s.Promotion.CategoryCouponName));
        }
        private static string ConvertTimeToDuration(DateTime timeStart, DateTime timeEnd)
        {
            if(DateTime.Today.Date < timeStart)
            {
                return CouponEnumData.COUPON_NOT_YET;
            }

            TimeSpan remaingTime = timeEnd.Date - DateTime.Today.Date;
            int daysRemaining = remaingTime.Days + 1;
            if (timeEnd.Date < DateTime.Today.Date)
            {
                return CouponEnumData.COUPON_EXPIRED;
            }
            if (daysRemaining == 1)
            {
                return CouponEnumData.COUPON_TODAY;
            }
            else if(daysRemaining >= 1)
            {
                return $"{daysRemaining}  days left";
            }
            return "";
        }
        private static bool AllowToShow(int status,int quantity, DateTime sDate, DateTime eDate)
        {
            if (status == 0 || status == 2)
                return false;
            if(quantity == 0)
                return false;
            var today = DateTime.Today;
            if (today >= sDate.Date && today <= eDate.Date)
                return true;
            return false;
        }
        private static bool CheckShowing(int status, int quantity, DateTime sDate, DateTime eDate)
        {
            if(status != 2) 
                return false;
            if (quantity == 0)
                return false;
            var today = DateTime.Today;
            if(today >= sDate.Date && today <= eDate.Date )
                return true;
            return false;
        }

        private static string Summary(decimal value, string promotion)
        {
            return promotion switch
            {
                DShopConst.FREE_SHIPPING => DShopConst.FREE_SHIPPING,
                DShopConst.NEW_CUSTOMER => DShopConst.DESCRIPTION_NEW_CUSTOMER,
                DShopConst.SUB_SUMTOTAL_DISCOUNT => $"Sub {value:#,##0 VND}",
                DShopConst.PERCENTAGE_DISCOUNT => $"Sub {value:#,##0 VND}",
                _ => string.Empty
            };
        }

    }
}
