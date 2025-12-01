using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models.Blog
{
    [Table("Post")]
    public class PostModel
    {
        [Key]
        public int Id { set; get; }

        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} must between {1} to {2} characters")]
        public string Title { set; get; }
        public string ShortDescription { set; get; }

        [StringLength(150, MinimumLength = 5, ErrorMessage = "{0} must between {1} to {2} characters")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Use only characters [a-z0-9-]")]
        public string Slug { set; get; }

        [Display(Name = "Content")]
        public string PostContent { set; get; }


        [Display(Name = "Date created")]
        public DateTime DateCreated { set; get; }

        public string UserIdCreate { get; set; }

        [ForeignKey("UserIdCreate")]
        public virtual AppUserModel CreateBy { get; set; }

        [Display(Name = "Date updated")]
        public DateTime DateUpdated { set; get; }

        public List<PostSubjectModel> PostSubjects { get; set; }

        public string Image { get; set; }
        public int Status { get; set; }
        public bool IsPin { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

        public virtual ICollection<CommentModel> Comments { get; set; }
        public virtual ICollection<LikeModel> Likes { get; set; }
    }
}
