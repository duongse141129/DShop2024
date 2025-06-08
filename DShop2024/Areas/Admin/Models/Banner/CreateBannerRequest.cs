using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Areas.Admin.Models.Banner
{
    public class CreateBannerRequest
    {
        [Required(ErrorMessage = "The {0} field is required")]
        public string BannerName { get; set; }
        public string? Description { get; set; }
        public string Image { get; set; }

        [NotMapped]
        [FileExtension]
        [Required(ErrorMessage = "The {0} field is required")]
        public IFormFile? ImageUpload { get; set; }
    }
}
