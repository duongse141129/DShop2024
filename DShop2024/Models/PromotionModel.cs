using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Promotion")]
	public class PromotionModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "The {0} field is required")]
		public string CategoryCouponName { get; set; }
		public string Description { get; set; }

		public int Status { get; set; }
	}
}
