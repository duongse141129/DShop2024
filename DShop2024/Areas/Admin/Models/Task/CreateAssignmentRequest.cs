using System.ComponentModel.DataAnnotations;

namespace DShop2024.Areas.Admin.Models.Task
{
    public class CreateAssignmentRequest
    {
        [Required(ErrorMessage = "The {0} field is required")]
        public int TaskId { get; set; }
        public string Date { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public TimeSpan TimeFrom { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public TimeSpan TimeTo { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string EmployeeUserID { get; set; }
        public string AssignmentDetails { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (TimeFrom > TimeTo)
        //    {
        //        yield return new ValidationResult("Time To must be greater than the Time From.", new[] { "EndDate" });
        //    }
        //}
    }
}
