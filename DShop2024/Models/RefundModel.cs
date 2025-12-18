using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Refund")]
    public class RefundModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string OrderCode { get; set; }
        public int? PaymentId { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public decimal Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? RefundDate { get; set; }
        public string TransactionId { get; set; }
        public string TransactionContent { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Reason { get; set; }
        public string Image { get; set; }
        public int Status { get; set; }

        [ForeignKey("PaymentId")]
        public virtual PaymentModel? Payment { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }


}
