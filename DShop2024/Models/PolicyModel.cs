using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Policy")]
    public class PolicyModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Description { get; set; }

        public int Status { get; set; }
    }
}
