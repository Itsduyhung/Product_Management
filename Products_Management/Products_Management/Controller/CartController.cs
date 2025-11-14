using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products_Management.DTOs.Request;
using Products_Management.Repository.Interface;
using Microsoft.Extensions.Logging;

namespace Products_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepo;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartRepository cartRepo, ILogger<CartController> logger)
        {
            _cartRepo = cartRepo;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartRequest request)
        {
            var userId = int.Parse(User.FindFirst("id").Value);
            await _cartRepo.AddToCartAsync(userId, request);
            return Ok("Added to cart successfully");
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = int.Parse(User.FindFirst("id").Value);
            var items = await _cartRepo.GetCartItemsAsync(userId);
            return Ok(items);
        }

        [HttpPut("update/{productId}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromQuery] int quantity)
        {
            var userId = int.Parse(User.FindFirst("id").Value);
            await _cartRepo.UpdateQuantityAsync(userId, productId, quantity);
            return Ok("Quantity updated");
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("id").Value);
                _logger.LogInformation("Removing product {ProductId} from cart for user {UserId}", productId, userId);
                
                await _cartRepo.RemoveFromCartAsync(userId, productId);
                
                _logger.LogInformation("Successfully removed product {ProductId} from cart for user {UserId}", productId, userId);
                return Ok("Item removed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart", productId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}