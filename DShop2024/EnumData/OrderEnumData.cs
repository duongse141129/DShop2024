namespace DShop2024.EnumData
{
	public class OrderEnumData
	{
		public enum StatusOrder
		{
			Cancel,
			New,
			Accepted,
			Delivery,
			Complete

		}
        public enum SpanStatusOrder
        {
            danger,
            primary,
            info,
            warning,
            success

        }
    }
}
