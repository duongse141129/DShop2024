using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Category")]
	public class CategoryModel
	{
		[Key]
		public int Id { get; set; }
		[Required( ErrorMessage ="Name category's can not null")]
		public string CategoryName { get; set; }

		public string? Description { get; set; }
		public string Slug { get; set; }
		public int Status { get; set; }
	
	}
}
