using Microsoft.EntityFrameworkCore;
using Products_Management.Model;

namespace Products_Management.Configuration
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ProductEntity> Entities { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<CartEntity> Carts { get; set; }
        public DbSet<CartItemEntity> CartItems { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EntityConfiguration());
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CartConfiguration());
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CartItemConfiguration());
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}