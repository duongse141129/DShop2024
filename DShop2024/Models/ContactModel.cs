using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Contact")]
    public class ContactModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter your name shop")]
        public string ShopName { get; set; }

        [Required( ErrorMessage = "Enter your local address")]
        public string Map { get; set; }
        [Required(ErrorMessage = "Enter your hotline")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Enter your email address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Enter your description")]
        public string Description { get; set; }

        public string LogoImg { get; set; }


        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
