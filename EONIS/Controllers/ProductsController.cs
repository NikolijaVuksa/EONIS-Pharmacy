using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EONIS.Data;
using EONIS.Models;
using EONIS.DTOs;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly PharmacyContext _context;

        public ProductsController(PharmacyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            var dtoList = products.Select(p => new ProductReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Rx = p.Rx,
                BasePrice = p.BasePrice,
                VatRate = p.VatRate,
                Manufacturer = p.Manufacturer,
                Category = p.Category,
                PriceWithVat = p.PriceWithVat
            });

            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductReadDto>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var dto = new ProductReadDto
            {
                Id = product.Id,
                Name = product.Name,
                Rx = product.Rx,
                BasePrice = product.BasePrice,
                VatRate = product.VatRate,
                Manufacturer = product.Manufacturer,
                Category = product.Category,
                PriceWithVat = product.PriceWithVat
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductReadDto>> CreateProduct(ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Rx = dto.Rx,
                BasePrice = dto.BasePrice,
                VatRate = dto.VatRate,
                Manufacturer = dto.Manufacturer,
                Category = dto.Category
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var readDto = new ProductReadDto
            {
                Id = product.Id,
                Name = product.Name,
                Rx = product.Rx,
                BasePrice = product.BasePrice,
                VatRate = product.VatRate,
                Manufacturer = product.Manufacturer,
                Category = product.Category,
                PriceWithVat = product.PriceWithVat
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, readDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = dto.Name;
            product.Rx = dto.Rx;
            product.BasePrice = dto.BasePrice;
            product.VatRate = dto.VatRate;
            product.Manufacturer = dto.Manufacturer;
            product.Category = dto.Category;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("list")]
        public async Task<ActionResult<PagedResultDto<ProductListItemDto>>> List(
             [FromQuery] int page = 1,
             [FromQuery] int pageSize = 10,
             [FromQuery] string? q = null,
             [FromQuery] string sortBy = "name",
             [FromQuery] string dir = "asc",
             [FromQuery] string? category = null,
             [FromQuery] bool? rx = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || p.Manufacturer.Contains(q));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            if (rx is not null)
                query = query.Where(p => p.Rx == rx);

            query = (sortBy, dir.ToLower()) switch
            {
                ("price", "desc") => query.OrderByDescending(p => p.BasePrice),
                ("price", _) => query.OrderBy(p => p.BasePrice),
                ("name", "desc") => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductListItemDto(p.Id, p.Name, p.Rx, p.BasePrice, p.VatRate, p.Manufacturer, p.Category))
                .ToListAsync();

            return Ok(new PagedResultDto<ProductListItemDto>(items, total, page, pageSize));
        }


    }
}
