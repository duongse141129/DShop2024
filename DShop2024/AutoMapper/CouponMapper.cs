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
                .ForMember(des => des.CategoryCouponName, s => s.MapFrom(s => s.Promotion.CategoryCouponName));
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
    }
}
