namespace DShop2024.EnumData
{
	public class Order
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
