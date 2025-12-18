

namespace DShop2024.ViewModels
{
    public class ReturnDetailViewModel 
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        public decimal PricePerUnit { get; set; }
        public string ReturnReason { get; set; }
        public int ReturnId { get; set; }
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string Description { get; set; }
        public decimal SubTotal { get { return Quantity * PricePerUnit; } }
        public List<string> Images { get; set; } = new();
    }
}
