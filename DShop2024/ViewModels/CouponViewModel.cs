using DShop2024.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.ViewModels
{
	public class CouponViewModel
	{
		public int Id { get; set; }
		public string CouponCode { get; set; }
		public string CouponName { get; set; }
		public decimal Value { get; set; }
		public string? Description { get; set; }
		public DateTime DateStart { get; set; }
		public DateTime DateExpired { get; set; }
		public int Quantity { get; set; }
		public int Status { get; set; }

		public string CategoryCouponName { get; set; }
		public int daysleft { get; set; }

	}
}
