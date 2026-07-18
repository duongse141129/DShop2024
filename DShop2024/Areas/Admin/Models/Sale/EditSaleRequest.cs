using DShop2024.Models;
using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.Areas.Admin.Models.Sale
{
    public class EditSaleRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [Range(1000, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        [VNDPriceAttribute(ErrorMessage = "The price must be a multiple of 1000.")] 
        public decimal SalePrice { get; set; }

        public DateTime SaleStartDate { get; set; }
        public DateTime SaleEndDate { get; set; }
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }
    }
}
