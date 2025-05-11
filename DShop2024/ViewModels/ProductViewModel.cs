
namespace DShop2024.ViewModels
{
	public class ProductViewModel
	{
		public int Id { get; set; }
		public string ProductName { get; set; }
		public string Slug { get; set; }
		public string Image { get; set; }
		public decimal Price { get; set; }
		public int Stock { get; set; }

		public string BrandName { get; set; }
		public string CategoryName { get; set; }

		public double AveragePoint { get; set; }
		public int QuantitySold { get; set; }
	}
}
