using System.IO;
using System.Threading.Tasks;
using EONIS.Configuration;
using EONIS.Data;
using EONIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly PharmacyContext _db;
        private readonly StripeSettings _settings;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            PharmacyContext db,
            IOptions<StripeSettings> options,
            ILogger<StripeWebhookController> logger)
        {
            _db = db;
            _settings = options.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Handle()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _settings.WebhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe signature verification failed.");
                return BadRequest();
            }

            try
            {
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await OnPaymentIntentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await OnPaymentIntentFailed(stripeEvent);
                        break;

                    case "charge.refunded":
                        await OnChargeRefunded(stripeEvent);
                        break;

                    case "checkout.session.completed":
                        await OnCheckoutSessionCompleted(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Stripe event {Type}", stripeEvent.Type);
                return StatusCode(500);
            }
        }

        private async Task OnPaymentIntentSucceeded(Event stripeEvent)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent == null) return;

            Payment? payment = await _db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);

            if (payment != null)
            {
                payment.Status = "Succeeded";

                var order = await _db.Orders.FindAsync(payment.OrderId);
                if (order != null) order.Status = "Paid";

                await _db.SaveChangesAsync();
                _logger.LogInformation("Order {OrderId} marked as Paid.", payment.OrderId);
            }
        }

        private async Task OnPaymentIntentFailed(Event stripeEvent)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent == null) return;

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);
            if (payment != null)
            {
                payment.Status = "Failed";

                var order = await _db.Orders.FindAsync(payment.OrderId);
                if (order != null) order.Status = "Payment Failed";

                await _db.SaveChangesAsync();
                _logger.LogInformation("Order {OrderId} marked as Payment Failed.", payment.OrderId);
            }
        }

        private async Task OnChargeRefunded(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null || string.IsNullOrEmpty(charge.PaymentIntentId)) return;

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == charge.PaymentIntentId);
            if (payment != null)
            {
                payment.Status = "Refunded";

                var order = await _db.Orders.FindAsync(payment.OrderId);
                if (order != null) order.Status = "Refunded";

                await _db.SaveChangesAsync();
                _logger.LogInformation("Order {OrderId} marked as Refunded.", payment.OrderId);
            }
        }

        private async Task OnCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

            if (!string.IsNullOrEmpty(session.PaymentIntentId))
            {
                var pi = await new PaymentIntentService().GetAsync(session.PaymentIntentId);
                var fakeEvent = new Event
                {
                    Type = "payment_intent.succeeded",
                    Data = new EventData { Object = pi }
                };
                await OnPaymentIntentSucceeded(fakeEvent);
            }
        }
    }
}
