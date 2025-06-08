using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.Models
{
    [Table("InformationShop")]
    public class InformationShopModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        public string ShopName { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        public string Map { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Description { get; set; }

        public string LogoImg { get; set; }
        public string PluginYoutube { get; set; }
        public string PluginFacebook { get; set; }


        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
