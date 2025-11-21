
using System.ComponentModel.DataAnnotations;

namespace DShop2024.ViewModels
{
	public class ProductViewModel 
	{
		public int Id { get; set; } 
		public string ProductName { get; set; }
		public string Slug { get; set; }
		public string MainImage { get; set; }
		public decimal Price { get; set; }
		public decimal OriginalPrice { get; set; }
		public int Stock { get; set; }

        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

		public double AveragePoint { get; set; }
		public int QuantitySold { get; set; }
        public int WishlistCount { get; set; }



        public int Capacity { get; set; }
        public string Dimension { get; set; }
        public decimal Weight { get; set; }
        public string Material { get; set; }
        public int MainPocket { get; set; }
        public bool WaterResistance { get; set; }
        public bool USBChargingPort { get; set; }
        public decimal? LaptopPocket { get; set; }

    }
}
