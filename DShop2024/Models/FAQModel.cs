using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("FAQ")]
	public class FAQModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "The {0} field is required")]
		public string Question { get; set; }
		[Required(ErrorMessage = "The {0} field is required")]
		public string Answer { get; set; }
		
	}
}
