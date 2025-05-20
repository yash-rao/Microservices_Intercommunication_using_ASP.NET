namespace OrderService.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    }
}
