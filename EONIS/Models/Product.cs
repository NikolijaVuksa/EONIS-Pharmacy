using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EONIS.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Name { get; set; } = string.Empty;

        public bool Rx { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        public decimal BasePrice { get; set; }

        [Range(0, 50)]
        public int VatRate { get; set; } = 10;

        [MaxLength(160)]
        public string Manufacturer { get; set; } = string.Empty;

        [MaxLength(120)]
        public string Category { get; set; } = string.Empty;

        [NotMapped]
        public decimal PriceWithVat => decimal.Round(BasePrice * (1 + VatRate / 100m), 2);

        public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();

    }
}
