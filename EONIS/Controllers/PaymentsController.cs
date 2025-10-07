using EONIS.DTOs.Payments;
using EONIS.Services;
using Microsoft.AspNetCore.Mvc;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripePaymentService _service;

        public PaymentsController(IStripePaymentService service)
        {
            _service = service;
        }

        //intent i vraca stripe odatke
        [HttpPost("create")]
        public async Task<ActionResult<PaymentCreateResponseDto>> CreatePayment([FromQuery] int orderId)
        {
            var result = await _service.CreatePaymentIntentAsync(orderId);
            return Ok(result);
        }

        //test bez webhook za uspesnu uplatu
        [HttpPost("confirm")]
        public async Task<ActionResult<PaymentCreateResponseDto>> ConfirmPayment([FromQuery] int orderId)
        {
            var result = await _service.ConfirmPaymentAsync(orderId);
            return Ok(result);
        }
    }
}
