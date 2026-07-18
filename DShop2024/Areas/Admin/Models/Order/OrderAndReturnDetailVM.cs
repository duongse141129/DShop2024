namespace DShop2024.Areas.Admin.Models.Order
{
    public class OrderAndReturnDetailVM 
    {
        public int ReturnDetailId { get; set; }

        public decimal Price { get; set; }
        public decimal UnitPrice { get; set; }


        public int OriginalQuantity { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }

        public int ReturnedQuantity { get; set; }
        public int NetQtyKept { get; set; }

        public string Status { get; set; }
    }
}
