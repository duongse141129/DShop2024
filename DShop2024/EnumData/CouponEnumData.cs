

namespace DShop2024.EnumData
{
    public class CouponEnumData
    {
        public static readonly string COUPON_EXPIRED = "Expired";
        public static readonly string COUPON_TODAY = "Today";
        public static readonly string COUPON_NOT_YET = "Not yet";

        public enum StatusCoupon
        {
            Deleted,
            Created,
            Showing

        }
        public enum BackGorund
        {
            success,
            danger,
            info,
            light,
            warning,
            secondary,


        }
        public static string getbgColor(string couponTypeName)
        {
            if(couponTypeName == DShopConst.FREE_SHIPPING)
            {
                return ""+ (BackGorund) 0;
            }
            if (couponTypeName == DShopConst.PERCENTAGE_DISCOUNT)
            {
                return "" + (BackGorund)1;
            }
            if (couponTypeName == DShopConst.SUB_SUMTOTAL_DISCOUNT)
            {
                return "" + (BackGorund)2;
            }
            return "";
        }

        public static string getbgStyle(string couponTypeName)
        {
            if (couponTypeName == DShopConst.FREE_SHIPPING)
            {
                return "free-shipping";
            }
            if (couponTypeName == DShopConst.PERCENTAGE_DISCOUNT)
            {
                return "percentage-discount";
            }
            if (couponTypeName == DShopConst.SUB_SUMTOTAL_DISCOUNT)
            {
                return "cash-discount";
            }
            return "";
        }
    }
}
