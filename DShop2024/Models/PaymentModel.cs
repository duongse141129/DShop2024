using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Payment")]
    public class PaymentModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(200, ErrorMessage = "The {0} field is required")]
        public string PaymentName { get; set; }
        public string Logo { get; set; }
        public bool IsPrepayment { get; set; }
        public int Status { get; set; }
    }
}
