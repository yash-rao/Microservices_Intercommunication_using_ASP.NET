using ProductService.Domain.Common;

namespace ProductService.Domain.Entities

{
    public class Product : IEntity

    {
        public int Id { get; set; }          // Primary key
        public string Name { get; set; }     // Product name
        public string Description { get; set; } // Description
        public decimal Price { get; set; }   // Price
        public int Stock { get; set; }       // Available quantity
        public string Category { get; set; } // Optional: for grouping
    }
}
