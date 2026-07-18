using DShop2024.Models;
using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Areas.Admin.Models.Refund
{
    public class UpdateRefundRequest
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string OrderCode { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? RefundDate { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string TransactionId { get; set; }
        public string TransactionContent { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Reason { get; set; } 
        public string Image { get; set; }
        public int Status { get; set; }


        [NotMapped]
        [FileExtension]
        [Required(ErrorMessage = "The {0} field is required")]
        public IFormFile? ImageUpload { get; set; }
    }
}
