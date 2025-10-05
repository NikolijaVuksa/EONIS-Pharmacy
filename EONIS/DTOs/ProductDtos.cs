namespace EONIS.DTOs
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public bool Rx { get; set; }
        public decimal BasePrice { get; set; }
        public int VatRate { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class ProductReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Rx { get; set; }
        public decimal BasePrice { get; set; }
        public int VatRate { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal PriceWithVat { get; set; }
    }

    public record PagedResultDto<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);

    public record ProductListItemDto(
        int Id,
        string Name,
        bool Rx,
        decimal BasePrice,
        int VatRate,
        string Manufacturer,
        string Category
    );
}
