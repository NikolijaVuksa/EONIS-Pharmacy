using EONIS.Data;
using EONIS.DTOs;
using EONIS.Models;
using EONIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly PharmacyContext _db;
        private readonly OrderService _svc;

        public OrdersController(PharmacyContext db, OrderService svc)
        {
            _db = db;
            _svc = svc;
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

        // kreiraj order sa vise stavki 
        [HttpPost]
        public async Task<ActionResult<OrderReadDto>> CreateOrder([FromBody] OrderCreateDto dto)
        {
            if (dto.Items is null || dto.Items.Count == 0)
                return BadRequest("Order must contain at least one item.");

            //da li svi proizvodi postoje
            var productIds = dto.Items.Select(it => it.ProductId).Distinct().ToList();
            var products = await _db.Products
                                    .Where(p => productIds.Contains(p.Id))
                                    .ToDictionaryAsync(p => p.Id);
            if (products.Count != productIds.Count)
                return BadRequest("One or more products do not exist.");

            // provara zaliha
            var requestedPerProduct = dto.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            foreach (var kv in requestedPerProduct)
            {
                if (kv.Value <= 0) return BadRequest("Quantity must be >= 1.");
                var enough = await _svc.HasSufficientStockAsync(kv.Key, kv.Value);
                if (!enough) return BadRequest($"Not enough stock for product {kv.Key}.");
            }

            // Draft order snapshot
            var order = new Order
            {
                Status = "Draft",
                CustomerEmail = dto.CustomerEmail,
                CreatedAt = DateTime.UtcNow,
                Items = dto.Items.Select(i => {
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

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, Map(order));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderReadDto>> GetOrder(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound();
            return Ok(Map(order));
        }

        //skini sa zaliha
        [HttpPost("{id:int}/place")]
        public async Task<ActionResult<OrderReadDto>> Place(int id)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound();
            if (order.Status != "Draft") return BadRequest("Order already placed.");

            // Finalna provera zaliha
            var itemsByProduct = order.Items.GroupBy(i => i.ProductId)
                                            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
            foreach (var kv in itemsByProduct)
            {
                var enough = await _svc.HasSufficientStockAsync(kv.Key, kv.Value);
                if (!enough) return BadRequest($"Not enough stock for product {kv.Key}.");
            }

            foreach (var kv in itemsByProduct)
                await _svc.DeductStockFifoAsync(kv.Key, kv.Value);

            order.Status = "Placed";
            await _db.SaveChangesAsync();

            var placed = await _db.Orders.Include(o => o.Items).FirstAsync(o => o.Id == id);
            return Ok(Map(placed));
        }

        [HttpPost("{id:int}/pay")]
        public async Task<ActionResult<OrderReadDto>> Pay(int id)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound();

            if (order.Status != "Placed")
                return BadRequest("Only placed orders can be marked as paid.");

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
                    var batch = await _db.StockBatches
                        .Where(b => b.ProductId == item.ProductId)
                        .OrderByDescending(b => b.ExpiryDate) // vrati u batch s najdaljim rokom
                        .FirstOrDefaultAsync();

                    if (batch != null)
                        batch.QuantityOnHand += item.Quantity;
                }
            }

            order.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return Ok(Map(order));
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            var email = User.Identity?.Name;
            if (email == null)
                return Unauthorized();

            var orders = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerEmail == email)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }


    }
}
