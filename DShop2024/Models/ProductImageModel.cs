using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("ProductImage")]
    public class ProductImageModel
    {
        [Key]
        public int Id { get; set; }
        public string ImagePath { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual ProductModel Product { get; set; }
    }
}
