using System.ComponentModel.DataAnnotations;

namespace DShop2024.ViewModels
{
	public class LoginViewModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "input your User Name")]
		public string UserName { get; set; }
		[DataType(DataType.Password), Required(ErrorMessage = "input your Password")]
		public string Password { get; set; }
		public string ReturnUrl { get; set; }
	}
}
