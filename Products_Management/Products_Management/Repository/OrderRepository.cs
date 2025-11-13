using Microsoft.EntityFrameworkCore;
using Products_Management.Configuration;
using Products_Management.Model;
using Products_Management.Repository.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Products_Management.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Tạo mới đơn hàng
        public async Task<OrderEntity> CreateOrderAsync(OrderEntity order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        // ✅ Lấy danh sách đơn hàng của user
        public async Task<IEnumerable<OrderEntity>> GetOrdersByUserAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // ✅ Lấy chi tiết 1 đơn hàng theo ID
        public async Task<OrderEntity?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        // ✅ Lấy đơn hàng theo OrderCode (PayOS)
        public async Task<OrderEntity?> GetOrderByCodeAsync(string orderCode)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        // ✅ Cập nhật trạng thái đơn hàng (Pending → Paid, Cancel, ...)
        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Cập nhật thông tin thanh toán từ PayOS
        public async Task UpdatePaymentInfoAsync(int orderId, string orderCode, string transactionId, string paymentLink)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderCode = orderCode;
                order.TransactionId = transactionId;
                order.PaymentLink = paymentLink;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Đánh dấu thanh toán hoàn tất
        public async Task MarkAsPaidAsync(string orderCode)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order != null)
            {
                order.Status = "Paid";
                order.PaidAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}