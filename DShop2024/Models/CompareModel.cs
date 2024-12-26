using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Compare")]
	public class CompareModel
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
