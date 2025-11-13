using Products_Management.DTOs.Request;
using Products_Management.Model;

namespace Products_Management.Repository.Interface
{
    public interface ICartRepository
    {
        Task AddToCartAsync(int userId, CartRequest request);
        Task<IEnumerable<CartItemResponse>> GetCartItemsAsync(int userId);
        Task UpdateQuantityAsync(int userId, int productId, int quantity);
        Task RemoveFromCartAsync(int userId, int productId);
    }
}