namespace OrderService.API.Models
{
    public class OrderSummaryDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
