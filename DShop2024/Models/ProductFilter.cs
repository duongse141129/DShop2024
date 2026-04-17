namespace DShop2024.Models
{
    public class ProductFilter  
    {
        public string CategorySlug { get; set; } = "";
        public List<string> BrandSlugs { get; set; } = new List<string>();
        public string SearchName { get; set; } = "";
        public string StartPrice { get; set; } = "50000";
        public string EndPrice { get; set; } = "2000000";
        public string LaptopPocket { get; set; } = "";
        public bool WaterResistance { get; set; } =false;
        public bool USBChargingPort { get; set; } = false;
        public int P { get; set; } = 1; 
        public int PagesSize { get; set; } = 9;
        public string SortByList { get; set; } = "";
    }
}
