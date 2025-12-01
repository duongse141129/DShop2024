using DShop2024.Models.Blog;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Like")]
    public class LikeModel
    {
        public string UserId { get; set; }
        public int PostId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUserModel User { get; set; }

        [ForeignKey("PostId")]
        public virtual PostModel Post { get; set; }
    }
}
