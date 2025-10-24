using DShop2024.Models;
using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.Areas.Admin.Models.Product
{
    public class UpdateProductRequest
    {
        [Key]
        public int Id { get; set; }
        [Required, MinLength(4, ErrorMessage = "The {0} field is required and must contain at least {1} characters")]
        public string ProductName { get; set; }
        public string Slug { get; set; }
        public string MainImage { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "The {0} field is required ")]
        [Range(1000, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Price { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Seclect a brand")]
        public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Seclect a brand")]
        public int CategoryId { get; set; }
        public BrandModel Brand { get; set; }
        public CategoryModel Category { get; set; }
        public RatingModel Rating { get; set; }

        [Required(ErrorMessage = "The {0} field is required ")]
        [Range(1000, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public int Capacity { get; set; }
        [Required, MaxLength(300, ErrorMessage = "The {0} field is required")]
        public string Dimension { get; set; }

        [Required(ErrorMessage = "The {0} field is required ")]
        [Range(0.1, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public decimal Weight { get; set; }
        [Required, MaxLength(300, ErrorMessage = "The {0} field is required")]
        public string Material { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public int MainPocket { get; set; }
        public bool WaterResistance { get; set; }
        public bool USBChargingPort { get; set; }


        public decimal? LaptopPocket { get; set; }


        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

        [NotMapped]
        [FileExtension]
        public List<ProductImageModel> ExistingImages { get; set; } = new();

        [NotMapped]
        [FileExtension]
        public List<IFormFile> ImageFiles { get; set; } = new();

    }
}
