using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("FAQ")]
	public class FAQModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Question can not null")]
		public string Question { get; set; }
		[Required(ErrorMessage = "Answer can not null")]
		public string Answer { get; set; }
		
	}
}
