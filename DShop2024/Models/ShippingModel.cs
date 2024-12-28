using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Shipping")]
	public class ShippingModel
	{
		[Key]
		public int Id { get; set; }
		public decimal Price { get; set; }
		public string Ward { get; set; }
		public string District { get; set; }
		public string City { get; set; }
		public int Status { get; set; }
	}
}
