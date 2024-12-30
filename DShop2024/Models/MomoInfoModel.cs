using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("MomoInfo")]
	public class MomoInfoModel
	{
		[Key]
		public int Id { get; set; }

		public string OrderId { get; set; } 
		public string OrderInfo { get; set; } 
		public string FullName { get; set; }
		public decimal Amount { get; set; }

		public DateTime DatePaid { get; set; }
	}
}
