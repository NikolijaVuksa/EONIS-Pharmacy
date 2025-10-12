using EONIS.Data;
using EONIS.DTOs;
using EONIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

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
                Description = p.Description,    // ✅ dodato
                ImagePath = p.ImagePath,        // ✅ dodato
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
                Description = product.Description, // ✅
                ImagePath = product.ImagePath,     // ✅
                PriceWithVat = product.PriceWithVat
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
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
                Category = dto.Category,
                Description = dto.Description,  // ✅
                ImagePath = dto.ImagePath
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
                Description = product.Description, // ✅
                ImagePath = product.ImagePath,     // ✅
                PriceWithVat = product.PriceWithVat
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, readDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updated)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = updated.Name ?? existing.Name;
            existing.Rx = updated.Rx;
            existing.BasePrice = updated.BasePrice != 0 ? updated.BasePrice : existing.BasePrice;
            existing.VatRate = updated.VatRate != 0 ? updated.VatRate : existing.VatRate;
            existing.Manufacturer = updated.Manufacturer ?? existing.Manufacturer;
            existing.Category = updated.Category ?? existing.Category;
            existing.Description = updated.Description ?? existing.Description; // ✅
            existing.ImagePath = updated.ImagePath ?? existing.ImagePath;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [Authorize(Roles = "Admin")]
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
                .Select(p => new ProductListItemDto(
                    p.Id,
                    p.Name,
                    p.Rx,
                    p.BasePrice,
                    p.VatRate,
                    p.Manufacturer,
                    p.Category,
                    p.ImagePath,     
                    p.Description    
                ))
                .ToListAsync();

            return Ok(new PagedResultDto<ProductListItemDto>(items, total, page, pageSize));
        }

        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile imageFile)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound($"Proizvod sa ID {id} nije pronađen.");

            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Niste poslali fajl.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{product.Id}_{Path.GetFileName(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            product.ImagePath = $"images/{fileName}";

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            product.ImagePath = $"images/{fileName}";

            _context.Attach(product);
            _context.Entry(product).Property(p => p.ImagePath).IsModified = true;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { imagePath = product.ImagePath });
        }
    }
}
