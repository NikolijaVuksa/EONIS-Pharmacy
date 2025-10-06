using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EONIS.Models
{
    public class Order
    {
        public int Id { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Pending"; 

        [EmailAddress]
        public string? CustomerEmail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    }
}
