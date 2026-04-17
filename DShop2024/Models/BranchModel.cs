using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Branch")]
    public class BranchModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public double Latitude { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public double Longitude { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Address { get; set; }
        public string MapEmbed { get; set; }
        public int Status { get; set; }
    }
}
