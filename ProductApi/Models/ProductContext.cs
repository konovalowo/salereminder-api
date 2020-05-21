using Microsoft.EntityFrameworkCore;

namespace ProductApi.Models
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfileProduct>()
                .HasKey(t => new { t.UserProfileId, t.ProductId });

            modelBuilder.Entity<UserProfileProduct>()
                .HasOne(sc => sc.UserProfile)
                .WithMany(s => s.UserProducts)
                .HasForeignKey(sc => sc.UserProfileId);

            modelBuilder.Entity<UserProfileProduct>()
                .HasOne(sc => sc.Product)
                .WithMany(c => c.UserProducts)
                .HasForeignKey(sc => sc.ProductId);
        }
    }
}
