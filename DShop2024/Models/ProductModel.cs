using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Product")]
	public class ProductModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MinLength(4, ErrorMessage = "Product name can not null and must > 4 characters")]
		public string ProductName { get; set; }
		public string Slug { get; set; }
        public string Image { get; set; }
        public string? Description { get; set; }
		[Required(ErrorMessage = "Product price can not null ")]
		[Range(1000, int.MaxValue, ErrorMessage = "Price > {1}")]
		[Column(TypeName ="decimal(8,2)")]
		public decimal Price { get; set; }
		public int Stock {  get; set; }
		[Required, Range(1, int.MaxValue, ErrorMessage ="Seclect a brand")]
		public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Seclect a brand")]
        public int CategoryId { get; set; }
		public BrandModel Brand { get; set; }
		public CategoryModel Category { get; set; }
		public RatingModel Rating { get; set; }
		public int Status { get; set; }

        [Required(ErrorMessage = "Original Price is required ")]
        [Range(1000, int.MaxValue, ErrorMessage = "Original Price > {1}")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Capacity is required ")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must > {1}")]
        public int Capacity { get; set; }
        [Required, MaxLength(300, ErrorMessage = "Dimension is required ")]
        public string Dimension { get; set; }

        [Required(ErrorMessage = "Weight is required  ")]
        [Range(0.1, int.MaxValue, ErrorMessage = "Weight must > {1}")]
        public decimal Weight { get; set; }
        [Required, MaxLength(300, ErrorMessage = "Material is required ")]
        public string Material { get; set; }
        [Required(ErrorMessage = "Compartment is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Compartment must >= {1}")]
        public int MainPocket { get; set; }
        public bool WaterResistance { get; set; }
        public bool USBChargingPort { get; set; }

      
        public decimal? LaptopPocket { get; set; }


        [NotMapped]
		[FileExtension]
        public IFormFile? ImageUpload { get; set; }

        public virtual ICollection<OrderDetailModel> OrderDetails { get; set; }

    }
}
