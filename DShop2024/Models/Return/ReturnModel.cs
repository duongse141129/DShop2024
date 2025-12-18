using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Return")]
    public class ReturnModel 
    {
        [Key]
        public int Id { get; set; }
        public string Reason { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }

        public DateTime UpdateDate { get; set; }
        public string UpdateUserId { get; set; }

        [ForeignKey("UpdateUserId")]
        public virtual AppUserModel UpdateBy { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUserModel Customer { get; set; }

        [ForeignKey("OrderId")]
        public virtual OrderModel Order { get; set; }
        [NotMapped]
        [FileExtension]
        public List<IFormFile> ImageFiles { get; set; } = new();
        public virtual ICollection<ReturnDetailModel> ReturnDetails { get; set; }
    }
}
