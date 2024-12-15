using Microsoft.AspNetCore.Mvc;
using Braintree;
using Flexiro.Contracts.Requests;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IBraintreeGateway _braintreeGateway;
        private readonly ILogger<PaymentController> _logger;
        private readonly ICartService _cartService;
        private readonly IPaymentService _paymentService;

        public PaymentController(ILogger<PaymentController> logger, IBraintreeGateway braintreeGateway,
            ICartService cartService, IPaymentService paymentService)
        {
            _logger = logger;
            _braintreeGateway = braintreeGateway;
            _cartService = cartService;
            _paymentService = paymentService;
        }

        [HttpPost("generate-client-token")]
        public async Task<IActionResult> GenerateClientToken()
        {
            try
            {
                var clientToken = await _braintreeGateway.ClientToken.GenerateAsync();
                return Ok(new { clientToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Braintree client token");
                return StatusCode(500, new { success = false, message = "Failed to generate client token." });
            }
        }

        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                // Fetch cart summary to get total amount
                var cartSummaryResult = await _cartService.GetCartSummaryAsync(request.OrderId);
                var totalAmount = cartSummaryResult.Content.Total;

                // Create transaction request
                var transactionRequest = new TransactionRequest
                {
                    Amount = totalAmount,
                    PaymentMethodNonce = request.PaymentMethodNonce,
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    },
                    OrderId = request.OrderId
                };

                // Process the transaction
                var result = await _braintreeGateway.Transaction.SaleAsync(transactionRequest);

                if (result.IsSuccess())
                {
                    // Return the result
                    return Ok(new
                    {
                        success = true,
                        transactionId = result.Target.Id,
                        amount = result.Target.Amount,
                        status = result.Target.Status
                    });
                }
                else
                {
                    _logger.LogError($"Braintree transaction failed: {result.Message}");
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Braintree payment");
                return StatusCode(500, new { success = false, message = "Failed to process payment." });
            }
        }
    }
}