using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Order")]
	public class OrderModel
	{
		public int Id { get; set; }
		public string OrderCode { get; set; }
		public string UserId { get; set; }
		public DateTime CreatedDate { get; set; }
		public string PaymentMethod { get; set; }
		public int Status { get; set; }

		[ForeignKey("UserId")]
		public virtual AppUserModel User { get; set; }

		public virtual ICollection<OrderDetailModel> OrderDetails { get; set; } 
	}
}
