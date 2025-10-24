using DShop2024.Models.Blog;
using System.ComponentModel.DataAnnotations;

namespace AppMvc.Areas.Blog.Models
{
    public class CreatePostModel: PostModel
    {
        [Display(Name= "Subjects")]
        public int[] SubjectIDs { get; set; }
    }
}
