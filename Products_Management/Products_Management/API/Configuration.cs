using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Products_Management.API
{
    public class EntityConfiguration : IEntityTypeConfiguration<Entity>
    {
        public void Configure(EntityTypeBuilder<Entity> builder)
        {
            // Correctly map the table name
            builder.ToTable("Entity");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(e => e.Description)
                   .HasColumnType("text");

            builder.Property(e => e.Price)
                   .HasColumnType("double precision")
                   .HasDefaultValue(0.0);

            builder.Property(e => e.ImageUrl)
                   .HasMaxLength(500);
        }
    }
}