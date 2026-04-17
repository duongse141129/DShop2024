namespace DShop2024.EnumData
{
    public class UserEnumData
    {
        public static readonly string LOGIN_WEBSITE = "WEBSITE";
        public static readonly string LOGIN_GMAIL = "GMAIL";
        public static readonly string IMAGE_DEFAULT = "imagesdefault.png";

        public enum StatusCustomerSegment
        {
            New ,
            Leads,
            Loyal,
            VIP
        }

        public enum RangeCustomerSegment
        {
            New = 0,
            Leads = 1000000,
            Loyal = 10000000,
            VIP   = 100000000
        }
    }
}
