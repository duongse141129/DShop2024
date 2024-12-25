using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	public class BannerModel
	{
		public int Id { get; set; }
		[Required( ErrorMessage = "Name banner's can not null")]
		public string BannerName { get; set; }
		public string? Description { get; set; }
		public string Image { get; set; }

		public int Status { get; set; }

		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}
