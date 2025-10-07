using System.ComponentModel.DataAnnotations;

namespace EONIS.Models
{
    public class PrescriptionReservation
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string? PrescriptionCode { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
