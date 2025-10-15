using EONIS.Data;
using EONIS.DTOs;
using EONIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly PharmacyContext _db;

        public OrdersController(PharmacyContext db)
        {
            _db = db;
        }

        private static OrderReadDto Map(Order o) => new OrderReadDto
        {
            Id = o.Id,
            Status = o.Status,
            CustomerEmail = o.CustomerEmail,
            CreatedAt = o.CreatedAt,

        Items = o.Items.Select(i => new OrderItemReadDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                VatRate = i.VatRate
            }).ToList()
        };

        // ✅ kreiranje order-a
        [HttpPost]
        public async Task<ActionResult<OrderReadDto>> CreateOrder([FromBody] OrderCreateDto dto)
        {
            if (dto.Items is null || dto.Items.Count == 0)
                return BadRequest("Order must contain at least one item.");

            var productIds = dto.Items.Select(it => it.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            if (products.Count != productIds.Count)
                return BadRequest("One or more products do not exist.");

            // provera da li ima dovoljno zaliha
            foreach (var item in dto.Items)
            {
                var p = products[item.ProductId];
                if (item.Quantity <= 0)
                    return BadRequest("Quantity must be >= 1.");
                if (p.TotalStock < item.Quantity)
                    return BadRequest($"Not enough stock for product {p.Name}.");
            }

            var order = new Order
            {
                Status = "Draft",
                CustomerEmail = dto.CustomerEmail,
                CreatedAt = DateTime.UtcNow,
                Items = dto.Items.Select(i =>
                {
                    var p = products[i.ProductId];
                    return new OrderItem
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Quantity = i.Quantity,
                        UnitPrice = p.BasePrice,
                        VatRate = p.VatRate
                    };
                }).ToList()
            };

            foreach (var item in dto.Items)
            {
                var p = products[item.ProductId];
                p.TotalStock -= item.Quantity; 
            }


            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, Map(order));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderReadDto>> GetOrder(int id)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound();
            return Ok(Map(order));
        }

        [HttpPost("{id:int}/place")]
        public async Task<ActionResult<OrderReadDto>> Place(int id)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound();
            if (order.Status != "Draft") return BadRequest("Order already placed.");

            foreach (var item in order.Items)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null) return BadRequest($"Product {item.ProductId} not found.");
                if (product.TotalStock < item.Quantity)
                    return BadRequest($"Not enough stock for product {product.Name}.");

                product.TotalStock -= item.Quantity; //skidanje zaliha
            }

            order.Status = "Placed";
            await _db.SaveChangesAsync();

            return Ok(Map(order));
        }

        [HttpPost("{id:int}/pay")]
        public async Task<ActionResult<OrderReadDto>> Pay(int id)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound();
            if (order.Status != "Placed") return BadRequest("Only placed orders can be marked as paid.");

            order.Status = "Paid";
            await _db.SaveChangesAsync();

            return Ok(Map(order));
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<OrderReadDto>> Cancel(int id)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound();

            if (order.Status == "Paid")
                return BadRequest("Cannot cancel an order that is already paid.");

            if (order.Status == "Cancelled")
                return BadRequest("Order is already cancelled.");

            if (order.Status == "Placed")
            {
                foreach (var item in order.Items)
                {
                    var product = await _db.Products.FindAsync(item.ProductId);
                    if (product != null)
                        product.TotalStock += item.Quantity; // ❗ vraćanje
                }
            }

            order.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return Ok(Map(order));
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value ??
                        User.FindFirst("email")?.Value ??
                        User.FindFirst(ClaimTypes.Name)?.Value;

            if (email == null) return Unauthorized("Nevažeći token.");

            var orders = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.CustomerEmail == email)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var result = orders.Select(o => new OrderReadDto
            {
                Id = o.Id,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemReadDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<AdminOrderReadDto>>> GetAllOrders()
        {
            var orders = await _db.Orders.Include(o => o.Items).OrderByDescending(o => o.CreatedAt).ToListAsync();
            var users = await _db.Users.ToListAsync();

            var dtos = orders.Select(o => new AdminOrderReadDto
            {
                Id = o.Id,
                Status = o.Status,
                CustomerEmail = o.CustomerEmail,
                CustomerName = users.FirstOrDefault(u => u.Email == o.CustomerEmail)?.FullName,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemReadDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    VatRate = i.VatRate
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = dto.Status;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
