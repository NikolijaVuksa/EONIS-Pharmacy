// EONIS/Models/Therapy.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EONIS.Models
{
    public class Therapy
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } 

        [Range(1, int.MaxValue)]
        public int Dosage { get; set; }              
        [MaxLength(16)]
        public string DosageUnit { get; set; } = "mg";  
        [MaxLength(64)]
        public string Frequency { get; set; } = "1x dnevno";
        [Range(1, 365)]
        public int DurationDays { get; set; } = 7;

        [MaxLength(64)]
        public string? PrescriptionCode { get; set; }   
        [MaxLength(128)]
        public string? DoctorName { get; set; }

        [MaxLength(24)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
