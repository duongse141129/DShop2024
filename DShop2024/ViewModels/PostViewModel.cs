using DShop2024.Models;
using System.ComponentModel.DataAnnotations;


namespace DShop2024.ViewModels
{
    public class PostViewModel 
    {
        public int Id { set; get; }
        public string Title { set; get; }
        public string ShortDescription { set; get; }
        public string Slug { set; get; }

        [Display(Name = "Content")]
        public string PostContent { set; get; }
        public DateTime DateCreated { get; set; }
        public AppUserModel Author { get; set; }

        [Display(Name = "Date updated")]
        public DateTime DateUpdated { set; get; }

        public string Image { get; set; }
        public bool IsPin { get; set; }
        public string Subjects { get; set; }
        public int CountLikes { get; set; }
        public int CountComments { get; set; }

    }
}
