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
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return;

            var item = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}