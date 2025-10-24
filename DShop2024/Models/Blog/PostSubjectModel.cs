using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models.Blog
{
    [Table("PostSubject")]
    public class PostSubjectModel
    {
        [Key]
        public int Id { get; set; }
        public int PostId { set; get; }

        public int SubjectId { set; get; }

        [ForeignKey("PostId")]
        public PostModel Post { set; get; }

        [ForeignKey("SubjectId")]
        public SubjectModel Subject { set; get; }
    }
}
