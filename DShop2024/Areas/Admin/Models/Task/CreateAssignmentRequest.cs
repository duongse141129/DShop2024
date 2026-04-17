using System.ComponentModel.DataAnnotations;

namespace DShop2024.Areas.Admin.Models.Task
{
    public class CreateAssignmentRequest
    {
        [Required(ErrorMessage = "The {0} field is required")]
        public int TaskId { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public TimeSpan TimeFrom { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public TimeSpan TimeTo { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string EmployeeUserID { get; set; }
        public string AssignmentDetails { get; set; }

    }
}
