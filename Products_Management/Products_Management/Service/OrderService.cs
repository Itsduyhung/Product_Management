using Microsoft.EntityFrameworkCore;
using Products_Management.Configuration;
using Products_Management.DTOs.Response;
using Products_Management.Model;
using Products_Management.Repository.Interface;
using Products_Management.Service.Interface;

namespace Products_Management.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;
        private readonly PayOSService _payOsService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ApplicationDbContext context,
            PayOSService payOsService,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _context = context;
            _payOsService = payOsService;
            _logger = logger;
        }

        // ==============================
        // 🧾 1. Tạo đơn hàng và payment link
        // ==============================
        public async Task<OrderResponse> PlaceOrderAsync(int userId)
        {
            // ✅ 1. Lấy giỏ hàng
            var cart = await GetUserCartAsync(userId);
            if (cart == null)
                throw new Exception("Cart not found");
            if (cart.Items == null || !cart.Items.Any())
                throw new Exception("Cart is empty");

            // ✅ 2. Tính tổng tiền và tạo entity
            var totalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity * 100);
            var order = BuildOrderEntity(userId, totalAmount, cart.Items);

            _logger.LogInformation("Creating new order for user {UserId} with total {Total}", userId, totalAmount);

            // ✅ 3. Xử lý trong transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // a. Lưu đơn hàng
                var savedOrder = await _orderRepository.CreateOrderAsync(order);

                // b. Tạo payment link PayOS
                var orderCode = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
                // PayOS requires description max 25 characters
                var description = $"Order #{orderCode}";
                var (paymentUrl, transactionId) = await _payOsService.CreatePaymentLinkAsync(orderCode, totalAmount, description);
                // c. Cập nhật link thanh toán
                await _orderRepository.UpdatePaymentInfoAsync(savedOrder.Id, orderCode, transactionId, paymentUrl);

                // d. Xóa giỏ hàng
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderId} placed successfully", savedOrder.Id);

                // ✅ 4. Build response
                var updatedOrder = await _orderRepository.GetOrderByIdAsync(savedOrder.Id);
                return MapOrderToResponse(updatedOrder!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed while placing order");
                await transaction.RollbackAsync();
                throw new Exception($"Failed to place order: {ex.Message}");
            }
        }

        // ==============================
        // 📦 2. Lấy danh sách đơn hàng
        // ==============================
        public async Task<IEnumerable<OrderResponse>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserAsync(userId);
            return orders.Select(MapOrderToResponse);
        }

        // ==============================
        // 📄 3. Lấy chi tiết đơn hàng
        // ==============================
        public async Task<OrderResponse?> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order == null ? null : MapOrderToResponse(order);
        }

        // ==============================
        // 🔍 4. Lấy đơn hàng theo OrderCode (PayOS)
        // ==============================
        public async Task<OrderResponse?> GetOrderByCodeAsync(string orderCode)
        {
            var order = await _orderRepository.GetOrderByCodeAsync(orderCode);
            return order == null ? null : MapOrderToResponse(order);
        }

        // ==============================
        // 🔄 5. Cập nhật trạng thái đơn hàng
        // ==============================
        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        }

        // ==============================
        // 💳 6. Xử lý callback từ PayOS
        // ==============================
        // Overload: Nhận status trực tiếp từ webhook request (nếu có)
        public async Task HandlePayOSCallbackAsync(string orderCode, string? webhookStatus = null)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order == null)
            {
                _logger.LogWarning("Order not found for orderCode: {OrderCode}", orderCode);
                throw new Exception($"Order not found: {orderCode}");
            }

            string? paymentStatus = null;

            // Nếu có status từ webhook request, dùng luôn (nhanh hơn)
            if (!string.IsNullOrWhiteSpace(webhookStatus))
            {
                paymentStatus = webhookStatus;
                _logger.LogInformation("Using status from webhook request: {Status}", paymentStatus);
            }
            else
            {
                // Nếu không có, verify từ PayOS API
                _logger.LogInformation("No status in webhook request, verifying from PayOS API...");
                try
                {
                    var paymentData = await _payOsService.VerifyPaymentAsync(orderCode);
                    
                    // PayOS SDK trả về PaymentLink object, cần extract status
                    // Cấu trúc: { code: "00", desc: "Success", data: { ... } }
                    if (paymentData.RootElement.TryGetProperty("data", out var dataEl))
                    {
                        // Thử các property names có thể có
                        if (dataEl.TryGetProperty("status", out var statusEl))
                            paymentStatus = statusEl.GetString();
                        else if (dataEl.TryGetProperty("Status", out var statusEl2))
                            paymentStatus = statusEl2.GetString();
                        else if (dataEl.TryGetProperty("state", out var stateEl))
                            paymentStatus = stateEl.GetString();
                        else if (dataEl.TryGetProperty("State", out var stateEl2))
                            paymentStatus = stateEl2.GetString();
                    }

                    if (string.IsNullOrWhiteSpace(paymentStatus))
                    {
                        _logger.LogWarning("Could not extract status from PayOS API response");
                        // Fallback: giữ nguyên status hiện tại
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying payment status from PayOS API");
                    throw;
                }
            }

            // Map PayOS status sang hệ thống status
            switch (paymentStatus?.ToUpperInvariant())
            {
                case "PAID":
                case "PAID_SUCCESS":
                    await _orderRepository.MarkAsPaidAsync(orderCode);
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, "Processing");
                    _logger.LogInformation("Order {OrderCode} marked as Paid and Processing", orderCode);
                    break;

                case "CANCELLED":
                case "CANCELLED_BY_USER":
                case "EXPIRED":
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, "Cancelled");
                    _logger.LogInformation("Order {OrderCode} marked as Cancelled", orderCode);
                    break;

                case "PENDING":
                case "WAITING_FOR_PAYMENT":
                    // Giữ nguyên status "Pending"
                    _logger.LogInformation("Order {OrderCode} remains Pending", orderCode);
                    break;

                default:
                    _logger.LogWarning("Unexpected payment status: {Status} for order {OrderCode}", paymentStatus, orderCode);
                    // Không throw exception, chỉ log warning để webhook vẫn trả về 200
                    break;
            }

            _logger.LogInformation("Handled PayOS callback for order {OrderCode} with status {Status}", orderCode, paymentStatus);
        }

        // ==============================
        // 🧩 Helper methods
        // ==============================
        private async Task<CartEntity?> GetUserCartAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private static OrderEntity BuildOrderEntity(int userId, double totalAmount, IEnumerable<CartItemEntity> items)
        {
            return new OrderEntity
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                Items = items.Select(i => new OrderItemEntity
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList()
            };
        }

        private static OrderResponse MapOrderToResponse(OrderEntity order)
        {
            return new OrderResponse
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderCode = order.OrderCode,
                PaymentUrl = order.PaymentLink,
                CreatedAt = order.OrderDate,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };
        }
    }
}