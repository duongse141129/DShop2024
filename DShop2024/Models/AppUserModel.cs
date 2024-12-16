using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.Models
{
	public class AppUserModel : IdentityUser
	{
		[Column(TypeName = "nvarchar")]
		[StringLength(100)]
		public string Occupation { get; set; }

		[Column(TypeName = "nvarchar")]
		[StringLength(400)]
		public string HomeAdress { get; set; }
    
		[DataType(DataType.Date)]
		public DateTime? BirthDate { get; set; }
	}
}
