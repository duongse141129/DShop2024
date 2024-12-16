using System.ComponentModel.DataAnnotations;

namespace DShop2024.Models
{
	public class UserModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage ="input your User Name")]
		public string UserName { get; set; }
		[Required(ErrorMessage = "input your User Email"),EmailAddress]
		public string Email { get; set; }
		[DataType(DataType.Password),Required(ErrorMessage = "input your Password")]
		public string Password { get; set; }
	}
}
