using EONIS.Data;
using EONIS.DTOs.Payments;
using EONIS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace EONIS.Services
{
    public interface IStripePaymentService
    {
        Task<PaymentCreateResponseDto> CreatePaymentIntentAsync(int orderId);
        Task<PaymentCreateResponseDto> ConfirmPaymentAsync(int orderId);
    }

    public class StripePaymentService : IStripePaymentService
    {
        private readonly PharmacyContext _db;
        private readonly StripeSettings _cfg;

        public StripePaymentService(PharmacyContext db, IOptions<StripeSettings> cfg)
        {
            _db = db;
            _cfg = cfg.Value;
        }

        // 🔹 1. Kreira Stripe PaymentIntent na osnovu izračunate vrednosti porudžbine
        public async Task<PaymentCreateResponseDto> CreatePaymentIntentAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new InvalidOperationException("Porudžbina nije pronađena.");

            // Izračunavanje ukupne sume
            decimal totalRsd = order.Items.Sum(i => i.Product.BasePrice * i.Quantity);
            long totalPara = (long)(totalRsd * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = totalPara,
                Currency = _cfg.Currency,
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string> { { "orderId", order.Id.ToString() } }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            // Upis u bazu
            var payment = new Payment
            {
                OrderId = order.Id,
                StripePaymentIntentId = intent.Id,
                Amount = totalPara,
                Currency = _cfg.Currency,
                Status = "Created"
            };

            _db.Payments.Add(payment);
            order.Status = "Payment Pending";

            await _db.SaveChangesAsync();

            return new PaymentCreateResponseDto
            {
                OrderId = order.Id,
                StripePaymentIntentId = intent.Id,
                ClientSecret = intent.ClientSecret,
                AmountRsd = totalRsd,
                Currency = _cfg.Currency,
                Status = payment.Status
            };
        }

        // 🔹 2. Simulira uspešno plaćanje (bez webhooka)
        public async Task<PaymentCreateResponseDto> ConfirmPaymentAsync(int orderId)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (payment == null)
                throw new InvalidOperationException("Plaćanje nije pronađeno za ovu porudžbinu.");

            payment.Status = "Succeeded";

            var order = await _db.Orders.FindAsync(orderId);
            if (order != null)
                order.Status = "Paid";

            await _db.SaveChangesAsync();

            return new PaymentCreateResponseDto
            {
                OrderId = orderId,
                StripePaymentIntentId = payment.StripePaymentIntentId,
                AmountRsd = payment.Amount / 100m,
                Currency = payment.Currency,
                Status = "Succeeded"
            };
        }
    }
}
