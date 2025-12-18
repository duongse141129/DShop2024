namespace DShop2024.ViewModels
{
    public class OrderDetailViewModel
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public String MainImage { get; set; }

        public decimal SubTotal { get { return Quantity * Price; } }
    }
}
