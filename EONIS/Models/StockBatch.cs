using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EONIS.Models
{
    public class StockBatch
    {
        public int Id { get; set; }

        // FK na Product
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required, MaxLength(64)]
        public string LotNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityOnHand { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
