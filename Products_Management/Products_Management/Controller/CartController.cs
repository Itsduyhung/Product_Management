using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products_Management.DTOs.Request;
using Products_Management.Repository.Interface;

namespace Products_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepo;

        public CartController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
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
            var userId = int.Parse(User.FindFirst("id").Value);
            await _cartRepo.RemoveFromCartAsync(userId, productId);
            return Ok("Item removed");
        }
    }
}