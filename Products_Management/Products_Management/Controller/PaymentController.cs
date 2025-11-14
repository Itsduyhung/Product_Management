using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products_Management.Repository.Interface;
using Products_Management.DTOs.Response;
using Products_Management.Service.Interface;
using Microsoft.Extensions.Logging;

namespace Products_Management.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IOrderService orderService,
            ILogger<PaymentController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOsWebhook([FromBody] DTOs.Request.PaymentCallbackRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OrderCode))
                {
                    _logger.LogWarning("Webhook received without OrderCode");
                    return BadRequest(new PayOsResponse(-1, "OrderCode is required", null));
                }

                _logger.LogInformation("Received PayOS webhook for order {OrderCode} with status {Status}", 
                    request.OrderCode, request.Status);

                // Truyền status từ webhook request (nếu có) vào handler
                // Nếu không có, handler sẽ verify từ PayOS API
                await _orderService.HandlePayOSCallbackAsync(request.OrderCode, request.Status);
                
                return Ok(new PayOsResponse(0, "Webhook processed successfully", null));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Webhook error for order {OrderCode}: {Message}", 
                    request?.OrderCode ?? "unknown", e.Message);
                return BadRequest(new PayOsResponse(-1, $"Invalid webhook data: {e.Message}", null));
            }
        }

        [HttpGet("status/{orderCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentStatus(string orderCode)
        {
            try
            {
                var order = await _orderService.GetOrderByCodeAsync(orderCode);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                return Ok(new
                {
                    orderCode = orderCode,
                    status = order.Status,
                    paymentUrl = order.PaymentUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for {OrderCode}", orderCode);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}