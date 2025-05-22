using InternPulse3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InternPulse3.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class paymentsController : ControllerBase
    {
        private readonly IPayStackService _paymentService;

        public paymentsController(IPayStackService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentRequest request)
        {
            try
            {
                var result = await _paymentService.InitializePayment(request);

                return Ok(new
                {
                    payment = result,
                    message = "Payment initialized successfully.",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = "failed",
                    message = $"Payment initialization failed: {ex.Message}"
                });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var payment = await _paymentService.GetByReferenceAsync(id);

            if (payment == null)
            {
                return NotFound(new
                {
                    status = "failed",
                    message = "Payment not found or failed to verify."
                });
            }

            return Ok(new
            {
                payment = payment,
                status = "success",
                message = "Payment status retrieved successfully.",
            });
        }

    }
}
