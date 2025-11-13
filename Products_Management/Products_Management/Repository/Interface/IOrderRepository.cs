using Products_Management.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Products_Management.Repository.Interface
{
    public interface IOrderRepository
    {
        Task<OrderEntity> CreateOrderAsync(OrderEntity order);
        Task<IEnumerable<OrderEntity>> GetOrdersByUserAsync(int userId);
        Task<OrderEntity?> GetOrderByIdAsync(int orderId);
        Task<OrderEntity?> GetOrderByCodeAsync(string orderCode);
        Task UpdateOrderStatusAsync(int orderId, string status);
        // Add methods used by OrderService for payment integration
        Task UpdatePaymentInfoAsync(int orderId, string orderCode, string transactionId, string paymentLink);
        Task MarkAsPaidAsync(string orderCode);
    }
}