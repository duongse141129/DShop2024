using System.ComponentModel.DataAnnotations;

namespace DShop2024.ViewModels
{
    public class StatisticalViewModel
    {
        [Key]
        public int Id { get; set; }
        public decimal revenue { get; set; }
        public int orders { get; set; }
        public int date { get; set; }
    }
}
