using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Task")]
    public class TaskModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(200, ErrorMessage = "The {0} field is required and maximum 200 characters")]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
    }
}
