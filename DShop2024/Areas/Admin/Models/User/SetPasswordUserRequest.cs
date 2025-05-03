using System.ComponentModel.DataAnnotations;

namespace DShop2024.Areas.Admin.Models.User
{
    public class SetPasswordUserRequest
    {
        [Required(ErrorMessage = "Input value {0}")]
        [StringLength(100, ErrorMessage = "{0} from {2} to {1} characters", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password", Prompt = "New Password")]
        public string NewPassword { get; set; }



        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password", Prompt = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Repeat password is incorrect.")]
        public string ConfirmPassword { get; set; }
    }
}
