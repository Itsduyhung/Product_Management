using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products_Management.Model;

namespace Products_Management.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
    {
        public void Configure(EntityTypeBuilder<OrderEntity> builder)
        {
            builder.ToTable("Order");

            // 🔑 Khóa chính
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                   .HasColumnName("IdOrder")
                   .ValueGeneratedOnAdd();

            builder.Property(o => o.UserId)
                   .HasColumnName("UserId")
                   .IsRequired();

            builder.Property(o => o.TotalAmount)
                   .HasColumnName("TotalAmount")
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(o => o.OrderDate)
                   .HasColumnName("OrderDate")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP")
                   .IsRequired();

            builder.Property(o => o.Status)
                   .HasColumnName("Status")
                   .HasMaxLength(50)
                   .HasDefaultValue("Pending");

            // 🧾 Thông tin thanh toán PayOS
            builder.Property(o => o.OrderCode)
                   .HasColumnName("OrderCode")
                   .HasMaxLength(100);

            builder.Property(o => o.PaymentLink)
                   .HasColumnName("PaymentLink")
                   .HasMaxLength(500);

            builder.Property(o => o.TransactionId)
                   .HasColumnName("TransactionId")
                   .HasMaxLength(200);

            builder.Property(o => o.PaidAt)
                   .HasColumnName("PaidAt");

            // 🔗 Quan hệ: Order 1 - N OrderItem
            builder.HasMany(o => o.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ⚡ Index giúp truy xuất nhanh theo UserId
            builder.HasIndex(o => o.UserId);
        }
    }
}