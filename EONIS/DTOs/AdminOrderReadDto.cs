namespace EONIS.DTOs
{
    public class AdminOrderReadDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemReadDto> Items { get; set; }
    }
    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }


}
