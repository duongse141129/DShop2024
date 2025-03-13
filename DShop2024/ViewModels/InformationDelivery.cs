using System.ComponentModel.DataAnnotations;

namespace DShop2024.ViewModels
{
	public class InformationDelivery
	{
		[Required( ErrorMessage = "Street is required ")]
		public string Street { get; set; }
		[Required(ErrorMessage = "Ward is required ")]
		public string phuong { get; set; }
		[Required(ErrorMessage = "District is required ")]
		public string quan { get; set; }
		[Required(ErrorMessage = "City is required ")]
		public string tinh { get; set; }

		public decimal ShippingCost { get; set; }


		[Required( ErrorMessage = "Phone Delivery is required ")]
		public string PhoneDelivery { get; set; }
		[Required(ErrorMessage = "Consignee is required ")]
		public string Consignee { get; set; }
	}
}
