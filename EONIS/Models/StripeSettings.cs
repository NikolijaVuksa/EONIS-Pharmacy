namespace EONIS.Models
{
    public sealed class StripeSettings
    {
        public string SecretKey { get; set; } = default!;
        public string PublishableKey { get; set; } = default!;
        public string Currency { get; set; } = "rsd";
    }
}
