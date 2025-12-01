using DShop2024.Models;
using DShop2024.Models.Blog;

namespace DShop2024.ViewModels
{
    public class PostDetailViewModel
    {
        public PostViewModel PostDetail { get; set; }
        public List<CommentViewModel> ListComments { get; set; } = new List<CommentViewModel>();
        public bool Liked { get; set; }


    }
}
