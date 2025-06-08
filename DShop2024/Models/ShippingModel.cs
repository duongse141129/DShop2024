using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Shipping")]
	public class ShippingModel
	{
		[Key]
		public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        [Range(10000, int.MaxValue, ErrorMessage = "Please input a number greater than {1}")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Province { get; set; }
	}
}
