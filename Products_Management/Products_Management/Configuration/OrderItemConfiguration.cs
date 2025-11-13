using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products_Management.Model;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.ToTable("OrderItem");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
               .HasColumnName("IdOrderItem")
               .ValueGeneratedOnAdd();

        builder.Property(i => i.OrderId)
               .HasColumnName("OrderId")
               .IsRequired();

        builder.Property(i => i.ProductId)
               .HasColumnName("ProductId")
               .IsRequired();

        builder.Property(i => i.Quantity)
               .HasColumnName("Quantity")
               .IsRequired();

        builder.Property(i => i.Price)
               .HasColumnName("Price")
               .HasPrecision(18, 2)
               .IsRequired();

        // 🔗 Quan hệ với Product (nếu có ProductEntity)
        builder.HasOne(i => i.Product)
               .WithMany()
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // ⚡ Index để truy vấn nhanh
        builder.HasIndex(i => i.OrderId);
        builder.HasIndex(i => i.ProductId);
    }
}