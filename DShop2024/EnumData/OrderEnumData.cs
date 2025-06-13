namespace DShop2024.EnumData
{
	public class OrderEnumData
	{
        public static int StatusOrderCount = 5;
		public enum StatusOrder
		{
			Cancel,
			New,
			Accepted,
			Delivery,
			Completed

		} 
        public enum SpanStatusOrder
        {
            danger,
            primary,
            info,
            warning,
            success

        }
        public static List<string> IconStatusOrder = new List<string> { "far fa-window-close", "fa fa-check", "fa fa-user", "fa fa-truck", "far fa-check-circle" };
    }
}
