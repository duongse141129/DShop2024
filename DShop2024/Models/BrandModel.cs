using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Brand")]
	public class BrandModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MinLength(4, ErrorMessage = "Name brand's can not null")]
		public string BrandName { get; set; }

		public string? Description { get; set; }
		public string Slug { get; set; }
		public int Status { get; set; }
	}
}
