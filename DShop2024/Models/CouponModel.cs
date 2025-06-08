using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Coupon")]
	public class CouponModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MaxLength(100, ErrorMessage = "The {0} field is required")]
		public string CouponCode { get; set; }

		[Required( ErrorMessage = "The {0} field is required")]
		public string CouponName { get; set; }
		[Required(ErrorMessage = "The {0} field is required")]
		[Range(0, int.MaxValue, ErrorMessage = "Value > {1}")]
		public decimal Value { get; set; }
		public string? Description { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public DateTime DateStart { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public DateTime DateExpired{ get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please input a number greater than 1")]
        public int Quantity { get; set; }
		public int Status { get; set; }
		public int PromotionId { get; set; }

		[ForeignKey("PromotionId")]
		public virtual PromotionModel Promotion { get; set; }



	}
}
