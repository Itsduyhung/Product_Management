using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Products_Management.DTOs.Request;
using Products_Management.DTOs.Response;
using Products_Management.Helper;
using Products_Management.Service.Interface;
using System.Security.Claims;

namespace Products_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bắt buộc có token
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // 🔔 Webhook endpoint for PayOS
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] PaymentCallbackRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OrderCode))
                {
                    return BadRequest(new { message = "OrderCode is required" });
                }

                await _orderService.HandlePayOSCallbackAsync(request.OrderCode);
                return Ok(new { message = "Payment processed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // 🧾 1️⃣ Đặt hàng từ giỏ (Place order)
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder()
        {
            _logger.LogInformation("=== Starting Place Order Process ===");
            _logger.LogDebug("Request Headers: {Headers}", 
                string.Join(", ", Request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value.ToArray())}")));

            try
            {
                var userId = User.GetUserId();
                _logger.LogInformation("Extracted UserId from token: {UserId}", userId);

                if (!userId.HasValue)
                {
                    _logger.LogWarning("Token invalid or missing UserId");
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
                }
                
                int userIdValue = userId.Value;
                _logger.LogInformation("Processing order for user {UserId}", userIdValue);

                var result = await _orderService.PlaceOrderAsync(userIdValue);
                _logger.LogInformation("Order placed successfully. Result: {@Result}", result);
                
                return Ok(new
                {
                    message = "Order placed successfully!",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order: {ErrorMessage}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerError}", ex.InnerException.Message);
                }
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // 📦 2️⃣ Lấy danh sách đơn hàng của user
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });

                int userId = int.Parse(userIdClaim.Value);

                var orders = await _orderService.GetOrdersByUserAsync(userId);
                return Ok(new
                {
                    message = "Fetched orders successfully!",
                    data = orders
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 🔍 3️⃣ Lấy chi tiết đơn hàng theo Id
        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound(new { message = "Order not found" });

                return Ok(new
                {
                    message = "Fetched order successfully!",
                    data = order
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class UpdateStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        [HttpPut("{orderId:int}/status")]
        [Authorize(Roles = "Admin")] // Giữ để sau test role
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Status))
                    return BadRequest(new { message = "Status is required" });

                await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
                return Ok(new { message = "Order status updated successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}