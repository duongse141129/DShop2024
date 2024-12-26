using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.Models
{
	[Table("WishList")]
	public class WishListModel
	{
		[Key]
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string UserId { get; set; }

		[ForeignKey("ProductId")]
		public virtual ProductModel Product { get; set; }

		[ForeignKey("UserId")]
		public virtual AppUserModel User { get; set; }
	}
}
