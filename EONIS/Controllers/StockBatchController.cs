using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EONIS.Data;
using EONIS.Models;
using EONIS.DTOs;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/[controller]")]
    public class StockBatchesController : ControllerBase
    {
        private readonly PharmacyContext _context;

        public StockBatchesController(PharmacyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockBatchReadDto>>> GetStockForProduct(int productId)
        {
            var batches = await _context.StockBatches
                .Where(b => b.ProductId == productId)
                .ToListAsync();

            var dtoList = batches.Select(b => new StockBatchReadDto
            {
                Id = b.Id,
                LotNumber = b.LotNumber,
                ExpiryDate = b.ExpiryDate,
                QuantityOnHand = b.QuantityOnHand,
                CreatedAt = b.CreatedAt
            });

            return Ok(dtoList);
        }

        [HttpGet("{batchId}")]
        public async Task<ActionResult<StockBatchReadDto>> GetStockBatch(int productId, int batchId)
        {
            var batch = await _context.StockBatches
                .FirstOrDefaultAsync(b => b.ProductId == productId && b.Id == batchId);

            if (batch == null) return NotFound();

            var dto = new StockBatchReadDto
            {
                Id = batch.Id,
                LotNumber = batch.LotNumber,
                ExpiryDate = batch.ExpiryDate,
                QuantityOnHand = batch.QuantityOnHand,
                CreatedAt = batch.CreatedAt
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<StockBatchReadDto>> AddStock(
            int productId, [FromBody] StockBatchCreateDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound($"Product {productId} not found.");

            var batch = new StockBatch
            {
                ProductId = productId,
                LotNumber = dto.LotNumber,
                ExpiryDate = dto.ExpiryDate,
                QuantityOnHand = dto.QuantityOnHand,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockBatches.Add(batch);
            await _context.SaveChangesAsync();

            var readDto = new StockBatchReadDto
            {
                Id = batch.Id,
                LotNumber = batch.LotNumber,
                ExpiryDate = batch.ExpiryDate,
                QuantityOnHand = batch.QuantityOnHand,
                CreatedAt = batch.CreatedAt
            };

            return CreatedAtAction(nameof(GetStockBatch),
                new { productId = productId, batchId = batch.Id }, readDto);
        }

        [HttpPut("{batchId}")]
        public async Task<IActionResult> UpdateStock(
            int productId, int batchId, [FromBody] StockBatchCreateDto dto)
        {
            var batch = await _context.StockBatches
                .FirstOrDefaultAsync(b => b.ProductId == productId && b.Id == batchId);

            if (batch == null) return NotFound();

            batch.LotNumber = dto.LotNumber;
            batch.ExpiryDate = dto.ExpiryDate;
            batch.QuantityOnHand = dto.QuantityOnHand;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{batchId}")]
        public async Task<IActionResult> DeleteStock(int productId, int batchId)
        {
            var batch = await _context.StockBatches
                .FirstOrDefaultAsync(b => b.ProductId == productId && b.Id == batchId);

            if (batch == null) return NotFound();

            _context.StockBatches.Remove(batch);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
