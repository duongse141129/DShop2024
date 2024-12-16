using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("OrderDetail")]
	public class OrderDetailModel
	{
		public int Id { get; set; }

		public decimal Price { get; set; }

		public int Quantity { get; set; }

		public int OrderId { get; set; }

		public int ProductId { get; set; }

		public int Status { get; set; }

		[ForeignKey("OrderId")]
		public virtual OrderModel Order { get; set; }

		[ForeignKey("ProductId")]
		public virtual ProductModel Product { get; set; }
	}
}
