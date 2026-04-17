using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Rating")]
	public class RatingModel 
	{
		[Key]
		public int Id { get; set; }

		[Column(TypeName = "nvarchar")]
		[StringLength(400)]
		public string? Comment { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1, 5, ErrorMessage = "Point from {1} to {2}")]
        public int Star { get; set; }
		public DateTime? RatingDateTime { get; set; }

		public int ProductId { get; set; }

		public string UserId { get; set; }
		public int Status { get; set; }


        public string? ReplyMessage { get; set; }
        public string UserIdReply { get; set; }



        [ForeignKey("ProductId")]
		public virtual ProductModel Product { get; set; }

		[ForeignKey("UserId")]
		public virtual AppUserModel User { get; set; }

        [ForeignKey("UserIdReply")]
        public virtual AppUserModel ReplyBy { get; set; }
    }

}
