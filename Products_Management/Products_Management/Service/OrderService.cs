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
                var description = $"Payment for Order #{orderCode}";
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
        public async Task HandlePayOSCallbackAsync(string orderCode)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order == null)
                throw new Exception($"Order not found: {orderCode}");

            var paymentData = await _payOsService.VerifyPaymentAsync(orderCode);
            var status = paymentData.RootElement.GetProperty("data").GetProperty("status").GetString();

            switch (status?.ToUpperInvariant())
            {
                case "PAID":
                    await _orderRepository.MarkAsPaidAsync(orderCode);
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, "Processing");
                    break;

                case "CANCELLED":
                case "EXPIRED":
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, "Cancelled");
                    break;

                default:
                    _logger.LogWarning("Unexpected payment status: {Status}", status);
                    throw new Exception($"Unexpected payment status: {status}");
            }

            _logger.LogInformation("Handled PayOS callback for order {OrderCode} with status {Status}", orderCode, status);
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