namespace EONIS.DTOs
{

    public class OrderCreateDto
    {
        public string? CustomerEmail { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }


    public class OrderItemCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }


    public class OrderItemReadDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int VatRate { get; set; }

        public decimal LineTotal => decimal.Round(UnitPrice * Quantity, 2);
        public decimal LineTotalWithVat => decimal.Round(UnitPrice * (1 + VatRate / 100m) * Quantity, 2);
    }

    // order koji se vraca customeru
    public class OrderReadDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = "Draft";
        public string? CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemReadDto> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public decimal TotalWithVat => Items.Sum(i => i.LineTotalWithVat);
    }
}
