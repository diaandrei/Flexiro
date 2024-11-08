using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Flexiro.Application.Models;

namespace Flexiro.Application.Database
{
    public class FlexiroDbContext : IdentityDbContext<ApplicationUser>
    {
        public FlexiroDbContext(DbContextOptions<FlexiroDbContext> options)
            : base(options) { }

        // DbSet properties
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<UserWishlist> UserWishlist { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // One-to-Many relationship between Cart and CartItems
            builder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many relationship between Order and OrderDetails
            builder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between Order and User
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-One relationship between Product and Category
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Many-to-One relationship between Product and Shop
            builder.Entity<Product>()
                .HasOne(p => p.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-One relationship between Order and ShippingAddress
            builder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between Order and BillingAddress
            builder.Entity<Order>()
                .HasOne(o => o.BillingAddress)
                .WithMany()
                .HasForeignKey(o => o.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between Review and Product
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between CartItem and Shop
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Shop)
                .WithMany()
                .HasForeignKey(ci => ci.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between OrderDetails and Shop
            builder.Entity<OrderDetails>()
                .HasOne(od => od.Shop)
                .WithMany()
                .HasForeignKey(od => od.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between UserWishlist and Shop
            builder.Entity<UserWishlist>()
                .HasOne(uw => uw.Shop)
                .WithMany()
                .HasForeignKey(uw => uw.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between UserWishlist and Product
            builder.Entity<UserWishlist>()
                .HasOne(uw => uw.Product)
                .WithMany()
                .HasForeignKey(uw => uw.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship between UserWishlist and ApplicationUser (User)
            builder.Entity<UserWishlist>()
                .HasOne(uw => uw.User)
                .WithMany()
                .HasForeignKey(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    }
}
