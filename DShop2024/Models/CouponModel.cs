using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Coupon")]
	public class CouponModel
	{
		[Key]
		public int Id { get; set; }

		[Required( ErrorMessage = "Coupon name can not null")]
		public string CouponName { get; set; }
		public string? Description { get; set; }
		public DateTime DateStart { get; set; }
		public DateTime DateExpired{ get; set; }
		public int Quantity { get; set; }
		public int Status { get; set; }



		
	}
}
