using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("ReceivingStock")]
    public class ReceivingStockModel
    {
        [Key]
        public int Id { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Please entering the quantity of the product received")]
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DateReceive { get; set; }
        public string UserId { get; set; }
        public int Status{ get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
        [ForeignKey("UserId")]
        public AppUserModel User { get; set; }


    }
}
