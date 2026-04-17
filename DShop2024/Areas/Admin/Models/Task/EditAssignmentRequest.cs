using System.ComponentModel.DataAnnotations;

namespace DShop2024.Areas.Admin.Models.Task
{
    public class EditAssignmentRequest
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string TaskID { get; set; }
        public string Date { get; set; } 
        [Required(ErrorMessage = "The {0} field is required")]
        public TimeSpan TimeFrom { get; set; } 
        [Required(ErrorMessage = "The {0} field is required")]
        public TimeSpan TimeTo { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string EmployeeUserID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeAvatar { get; set; }
        public string AssignmentDetails { get; set; }
        public int Status { get; set; }
    }
}
