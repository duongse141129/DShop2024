using System.ComponentModel.DataAnnotations;


namespace App.Areas.Identity.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "The {0} field is required")]
        [EmailAddress(ErrorMessage = "Must be in correct email format")]
        [Display(Name = "Email", Prompt = "Email")]
        public string Email { get; set; }


        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(100, ErrorMessage = "{0} must have at least {2} characteres.", MinimumLength = 2)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password", Prompt = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Incorrect Confirm Password.")]
        public string ConfirmPassword { get; set; }


        [DataType(DataType.Text)]
        [Display(Name = "UserName", Prompt = "User Name")]
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(100, ErrorMessage = "{0} must have at least {2} characteres.", MinimumLength = 3)]
        public string UserName { get; set; }

    }
}
