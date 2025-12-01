using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class CommentViewModel
    { 
        public int Id { get; set; }
        public string CommentContent { get; set; }
        public DateTime Timestamp { get; set; }
        public int PostId { get; set; }
        public UserViewModel User { get; set; }
        public int level { get; set; }

        public int? ParentCommentId { get; set; }
        public List<CommentViewModel> CommentChildren { get; set; } = new();
    }
}
