using System.ComponentModel.DataAnnotations;

namespace DShop2024.Areas.Admin.Models.User
{
	public class CreateUserRequest
	{
		[Required(ErrorMessage = "The {0} field is required")]
		[EmailAddress(ErrorMessage = "Wrong format Email")]
		[Display(Name = "Email", Prompt = "Email")]
		public string Email { get; set; }

		  
		[Required(ErrorMessage = "The {0} field is required")]
		[StringLength(100, ErrorMessage = "{0} must cointain from {2} to {1} characters", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password", Prompt = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm Password", Prompt = "Confirm Password")]
		[Compare("Password", ErrorMessage = "Repeat password is incorrect.")]
		public string ConfirmPassword { get; set; }


		[DataType(DataType.Text)]
		[Display(Name = "User Name", Prompt = "User Name")]
		[Required(ErrorMessage = "The {0} field is required")]
		[StringLength(100, ErrorMessage = "{0} must cointain from {2} to {1} characters.", MinimumLength = 3)]
		public string UserName { get; set; }

		[DataType(DataType.Text)]
		[Display(Name = "Role", Prompt = "Role")]
		[Required(ErrorMessage = "Select role")]
		public string RoleId { get; set; }
	}
}
