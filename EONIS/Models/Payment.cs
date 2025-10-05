using System.ComponentModel.DataAnnotations;

namespace EONIS.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        [MaxLength(100)]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "rsd";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
