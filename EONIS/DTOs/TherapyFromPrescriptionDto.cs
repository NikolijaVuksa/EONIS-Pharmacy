// EONIS/DTOs/TherapyFromPrescriptionDto.cs
using System;

namespace EONIS.DTOs
{
    public class TherapyFromPrescriptionDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }    
        public int Dosage { get; set; }
        public string DosageUnit { get; set; } = "mg";
        public string Frequency { get; set; } = "1x dnevno";
        public int DurationDays { get; set; } = 7;

        public string? PrescriptionCode { get; set; }
        public string? DoctorName { get; set; }
    }

    public class TherapyReadDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public int Dosage { get; set; }
        public string DosageUnit { get; set; } = "mg";
        public string Frequency { get; set; } = "1x dnevno";
        public int DurationDays { get; set; } = 7;

        public string? PrescriptionCode { get; set; }
        public string? DoctorName { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
