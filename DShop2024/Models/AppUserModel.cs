using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DShop2024.Repository.Validation;

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

		public string RoleId { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(500)]
        public string? Avatar { get; set; }

		public bool? sex { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(100)]
        public string loginType { get; set; }



		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }

		public int Status { get; set; }
    }
}
