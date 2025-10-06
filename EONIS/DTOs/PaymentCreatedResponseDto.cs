namespace EONIS.DTOs.Payments
{
    public class PaymentCreateResponseDto
    {
        public int OrderId { get; set; }
        public string StripePaymentIntentId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;
        public decimal AmountRsd { get; set; }
        public string Currency { get; set; } = "rsd";
        public string Status { get; set; } = "Created";
    }
}
