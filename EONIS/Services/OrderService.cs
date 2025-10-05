using EONIS.Data;
using Microsoft.EntityFrameworkCore;

namespace EONIS.Services
{
    public class OrderService
    {
        private readonly PharmacyContext _db;
        public OrderService(PharmacyContext db) => _db = db;

        // suma svih batchs za proizvod
        public async Task<int> GetTotalOnHandAsync(int productId)
        {
            return await _db.StockBatches
                .Where(b => b.ProductId == productId)
                .SumAsync(b => b.QuantityOnHand);
        }

        // provera stanja na lageru
        public async Task<bool> HasSufficientStockAsync(int productId, int desiredQty)
        {
            if (desiredQty <= 0) return false;
            var total = await GetTotalOnHandAsync(productId);
            return total >= desiredQty;
        }

        public async Task DeductStockFifoAsync(int productId, int qtyToDeduct)
        {
            var remaining = qtyToDeduct;
            var batches = await _db.StockBatches
                .Where(b => b.ProductId == productId && b.QuantityOnHand > 0)
                .OrderBy(b => b.ExpiryDate).ThenBy(b => b.Id)
                .ToListAsync();

            foreach (var b in batches)
            {
                if (remaining <= 0) break;
                var take = Math.Min(b.QuantityOnHand, remaining);
                b.QuantityOnHand -= take;
                remaining -= take;
            }

            if (remaining > 0)
                throw new InvalidOperationException("Insufficient stock while deducting FIFO.");

            await _db.SaveChangesAsync();
        }
    }
}
