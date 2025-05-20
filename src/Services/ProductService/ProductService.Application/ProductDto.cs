namespace ProductService.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; } // For update & delete
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
    }
}
