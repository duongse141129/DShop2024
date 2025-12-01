using DShop2024.Models.Blog;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Comment")]
    public class CommentModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100, ErrorMessage = "The {0} field is required and maximum 100 characters")]
        public string CommentContent { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUserModel User { get; set; }

        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual PostModel Post { get; set; }
        public int Status { get; set; }



        [Display(Name = "Parent Comment")]
        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        [Display(Name = "Parent Comment")]
        public CommentModel ParentComment { set; get; }

        public ICollection<CommentModel> CommentChildren { get; set; }


    }
}
