using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Statistical")]
    public class StatisticalModel
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; } // số lượng bán
        public int Sold { get; set; } // số lượng đơn hàng
        public int Revenue { get; set; }
        public int Profit { get; set; }
        public DateTime DateCreate { get; set; }

    }
}
