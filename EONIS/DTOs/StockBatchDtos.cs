namespace EONIS.DTOs
{
    public class StockBatchCreateDto
    {
        public string LotNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int QuantityOnHand { get; set; }
    }

    public class StockBatchReadDto
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int QuantityOnHand { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
