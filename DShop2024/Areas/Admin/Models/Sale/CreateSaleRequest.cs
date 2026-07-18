using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Areas.Admin.Models.Sale
{
    public class CreateSaleRequest
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1000, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        [Column(TypeName = "decimal(18,2)")]
        [VNDPriceAttribute(ErrorMessage = "The price must be a multiple of 1000.")]
        public decimal SalePrice { get; set; }

        public DateTime SaleStartDate { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        public DateTime SaleEndDate { get; set; }

    }
}
