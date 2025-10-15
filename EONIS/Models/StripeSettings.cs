namespace EONIS.Configuration
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string Currency { get; set; } = "rsd";
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
