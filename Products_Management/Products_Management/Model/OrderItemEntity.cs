using Products_Management.Model;
namespace Products_Management.Model
{

    public class OrderItemEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        public OrderEntity Order { get; set; }
        public ProductEntity Product { get; set; }
    }
}