namespace DShop2024.Areas.Admin.Models.Sale
{
    public class CreateSalesRequest
    {
        public DateTime SaleStartDate { get; set; }

        public DateTime SaleEndDate { get; set; }

        public List<CreateSaleItem> Items { get; set; } = [];
    }
    public class CreateSaleItem
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }

        public decimal SalePrice { get; set; }
    }
}
