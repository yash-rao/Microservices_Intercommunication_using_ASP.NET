using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Data;
using OrderService.API.Models;
using System.Net.Http;

namespace OrderService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get All Orders (Detailed Receipts)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll()
        {
            var orders = await _context.Orders.Include(o => o.Items).ToListAsync();

            var result = orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                TotalOrderPrice = o.Items.Sum(i => i.TotalPrice),
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // Get Single Order by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> Get(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var result = new OrderResponseDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalOrderPrice = order.Items.Sum(i => i.TotalPrice),
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            return Ok(result);
        }

        // Create Order
        [HttpPost]
        public async Task<ActionResult<Order>> Create([FromBody] Order order)
        {
            if (order.Items == null || !order.Items.Any())
                return BadRequest("At least one order item is required.");

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5163") // ProductService API
            };

            var mergedItems = order.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new OrderItem
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .ToList();

            var finalItems = new List<OrderItem>();

            foreach (var item in mergedItems)
            {
                try
                {
                    var response = await client.GetAsync($"/api/product/{item.ProductId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("ProductService returned error: {StatusCode}", response.StatusCode);
                        return BadRequest($"Invalid ProductId: {item.ProductId}");
                    }

                    var product = await response.Content.ReadFromJsonAsync<ProductDto>();
                    if (product == null)
                        return BadRequest($"Product not found: {item.ProductId}");

                    item.ProductName = product.Name;
                    item.UnitPrice = product.Price;

                    finalItems.Add(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling ProductService");
                    return StatusCode(500, "Error retrieving product details.");
                }
            }

            order.OrderDate = DateTime.UtcNow;
            order.Items = finalItems;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        // Delete Order by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
