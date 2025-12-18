namespace DShop2024.EnumData
{
	public class  OrderEnumData
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

        public enum PaymentStatusOrder
        {
            UnPaid,
            Paid,
            Refunded
        }
        public enum StatusReturn
        {
            Rejected,
            Pending,
            Accepted,
            Refunded
        }

        public enum StatusRefund
        {
            Rejected,
            Approved,
            Refunded
        }

        public static readonly string FULLY_RETURNED = "Fully Returned";
        public static readonly string PARTIALLY_RETURNED = "Partially Returned";
        public static readonly string KEPT = "Kept";

        public static readonly string REASON_REFUND_RETURN = "Return items";
        public static readonly string REASON_REFUND_CANCEL = "Cancel order";

        public static readonly List<string> IconStatusOrder = new List<string> { "far fa-window-close", "fa fa-check", "fa fa-user", "fa fa-truck", "far fa-check-circle" };
    }
}
