using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("ReturnDetail")] 
    public class ReturnDetailModel 
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1000, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public decimal PricePerUnit { get; set; }
        [Range(1000, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public decimal OriginalPricePerUnit { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        public string ReturnReason { get; set; }
        public int ReturnId { get; set; }
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string Images { get; set; }
        public string Description { get; set; }

        [ForeignKey("ReturnId")]
        public virtual ReturnModel Return { get; set; }
        [ForeignKey("ProductId")]
        public virtual ProductModel Product { get; set; }
        [NotMapped]
        [FileExtension]
        public List<IFormFile> ImageFiles { get; set; } = new();
    }
}
