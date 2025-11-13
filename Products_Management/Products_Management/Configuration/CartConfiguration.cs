using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products_Management.Model;

namespace Products_Management.Configuration
{
    public class CartConfiguration : IEntityTypeConfiguration<CartEntity>
    {
        public void Configure(EntityTypeBuilder<CartEntity> builder)
        {
            builder.ToTable("Cart");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId)
                   .IsRequired();

            builder.Property(c => c.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 1 Cart có nhiều CartItems
            builder.HasMany(c => c.Items)
                   .WithOne(ci => ci.Cart)
                   .HasForeignKey(ci => ci.CartId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}