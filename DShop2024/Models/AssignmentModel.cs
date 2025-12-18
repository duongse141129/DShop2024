using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Assignment")]
    public class AssignmentModel
    {
        [Key]
        public int Id { get; set; }
        public string AssignmentDetails { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public DateTime AssignedDate { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public DateTime Deadline { get; set; }
        public string AssignedByUserID { get; set; }
        public string EmployeeUserID { get; set; }
        public int TaskID { get; set; }
        public int Status { get; set; }


        [ForeignKey("AssignedByUserID")]
        public virtual AppUserModel AssignedBy { get; set; }

        [ForeignKey("EmployeeUserID")]
        public virtual AppUserModel Employee { get; set; }

        [ForeignKey("TaskID")]
        public virtual TaskModel Task { get; set; }


    }
}
