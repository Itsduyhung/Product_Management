using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Products_Management.Model
{
    [Table("CartItem")]
    public class CartItemEntity
    {
        [Key]
        public int Id { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        [ForeignKey("CartId")]
        public CartEntity Cart { get; set; }
        [ForeignKey("ProductId")]
        public ProductEntity Product { get; set; }

    }
}