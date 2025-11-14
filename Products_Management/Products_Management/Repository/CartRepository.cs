using Microsoft.EntityFrameworkCore;
using Products_Management.Configuration;
using Products_Management.DTOs.Request;
using Products_Management.Model;
using Products_Management.Repository.Interface;

namespace Products_Management.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        //private readonly ProductEntity _productsentity;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
            //_productsentity = productEntity;
        }

        public async Task AddToCartAsync(int userId, CartRequest request)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new CartEntity { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Entities.FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItemEntity
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Price = product.Price,
                });
            }

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<CartItemResponse>> GetCartItemsAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Enumerable.Empty<CartItemResponse>();

            return cart.Items.Select(ci => new CartItemResponse
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Price,
                ProductName = ci.Product.Name
            });
        }

        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return;

            var product = await _context.Entities.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var item = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                item.Price =  product.Price * quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            // Tìm cart của user
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                throw new Exception($"Cart not found for user {userId}");
            }

            // Tìm cart item cần xóa - dùng AsTracking() để đảm bảo EF Core track entity
            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);
            
            if (item == null)
            {
                throw new Exception($"Cart item not found for productId {productId} in cart {cart.Id}");
            }

            // Remove item - đảm bảo entity được track đúng cách
            // Cách 1: Dùng Remove() - EF Core sẽ tự động mark entity là Deleted
            _context.CartItems.Remove(item);
            
            // Cách 2: Hoặc dùng Entry().State = EntityState.Deleted (nếu cách 1 không work)
            // _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            
            // Save changes và kiểm tra rows affected
            var rowsAffected = await _context.SaveChangesAsync();
            
            // Kiểm tra xem có save được không
            if (rowsAffected == 0)
            {
                // Nếu không có rows affected, có thể entity không được track đúng
                // Thử force reload và remove lại
                await _context.Entry(item).ReloadAsync();
                if (await _context.CartItems.AnyAsync(ci => ci.Id == item.Id))
                {
                    // Nếu item vẫn còn, thử remove bằng cách khác
                    var itemToRemove = await _context.CartItems.FindAsync(item.Id);
                    if (itemToRemove != null)
                    {
                        _context.CartItems.Remove(itemToRemove);
                        rowsAffected = await _context.SaveChangesAsync();
                        
                        if (rowsAffected == 0)
                        {
                            throw new Exception($"Failed to remove cart item after retry. CartId: {cart.Id}, ProductId: {productId}, ItemId: {item.Id}");
                        }
                    }
                }
            }
        }
    }
}