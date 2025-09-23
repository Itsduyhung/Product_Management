namespace Products_Management.API
{
    public class EntityResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}