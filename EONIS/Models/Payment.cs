using System;

namespace EONIS.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string StripePaymentIntentId { get; set; } = default!;
        public string Status { get; set; } = "Pending";
        public long Amount { get; set; } 
        public string Currency { get; set; } = "rsd";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
