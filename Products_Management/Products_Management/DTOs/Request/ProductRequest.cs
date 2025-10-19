using Microsoft.AspNetCore.Http;

namespace Products_Management.DTOs.Request
{
    public class EntityRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Price { get; set; }
        public required IFormFile ImageUrl { get; set; }
    }
}
