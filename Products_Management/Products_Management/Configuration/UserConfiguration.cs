using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products_Management.Model;

namespace Products_Management.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            // Tên bảng
            builder.ToTable("users");

            // Khóa chính
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .ValueGeneratedOnAdd();

            // Email
            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            // PasswordHash
            builder.Property(u => u.Password)
                   .IsRequired()
                   .HasColumnType("text");

            // Ngày tạo
            builder.Property(u => u.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}