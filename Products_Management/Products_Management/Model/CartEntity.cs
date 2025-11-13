using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Products_Management.Model
{
    [Table("Cart")]
    public class CartEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItemEntity> Items { get; set; }
    }
}
