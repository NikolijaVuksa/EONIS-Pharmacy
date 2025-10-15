using System.ComponentModel.DataAnnotations;

namespace EONIS.Models
{
    public class StripeEventLog
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public string EventId { get; set; } = string.Empty;   
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;     
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
