using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using System;
using System.Diagnostics.Metrics;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace DShop2024.EnumData
{
    public class CouponEnumData
    {
        public static string COUPON_EXPIRED = "Expired";
        public static string COUPON_TODAY = "Today";
        public static string COUPON_NOT_YET = "Not yet";

        public enum StatusCoupon
        {
            Deleted,
            New,
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
        public static string getRandom()
        {
            Random rd = new Random();
            int count = Enum.GetNames(typeof(BackGorund)).Length;
            string bg = ""+ (BackGorund)rd.Next(1, count);
            return bg;
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
    }
}
