namespace EONIS.DTOs
{
    public class ReservationCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? PrescriptionCode { get; set; }
    }

    public class ReservationReadDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? PrescriptionCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
