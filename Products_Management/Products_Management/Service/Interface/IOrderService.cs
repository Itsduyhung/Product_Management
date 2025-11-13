using System.Collections.Generic;
using System.Threading.Tasks;
using Products_Management.DTOs.Response;

namespace Products_Management.Service.Interface
{
    public interface IOrderService
    {
        Task<OrderResponse> PlaceOrderAsync(int userId);
        Task<IEnumerable<OrderResponse>> GetOrdersByUserAsync(int userId);
        Task<OrderResponse?> GetOrderByIdAsync(int orderId);
        Task<OrderResponse?> GetOrderByCodeAsync(string orderCode);
        Task UpdateOrderStatusAsync(int orderId, string status);
        Task HandlePayOSCallbackAsync(string orderCode);
    }
}