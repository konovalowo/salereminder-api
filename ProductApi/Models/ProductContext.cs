using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ProductApi;
using ProductApi.Models;

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
            modelBuilder.Entity<Product>().HasData(
                new Product[]
                {
                    new Product {Id=1, Url="someurl1", Name="Pink boots", Brand="Bootsy",
                        Currency="RUB", Price=8999, IsOnSale=false, Description="Pink boots from Bootsy Inc.",
                        Image="https://i.ebayimg.com/images/g/TbIAAOSwo~dcYbCy/s-l300.jpg"},
                    new Product {Id=2, Url="someurl2", Name="Sand pants", Brand="Sandy's",
                        Currency="RUB", Price=4999, IsOnSale=false, Description="Sand pants from Sandy's Inc.",
                        Image="https://21-shop.ru/upload/resize/138/138477842/2000x2000x90/bryuki-skills-asymmetric-pants-sand.jpg"},
                    new Product {Id=3, Url="someurl3", Name="Stratocaster", Brand="Fender",
                        Currency="USD", Price=699, IsOnSale=false, Description="Surf green Fender Stratocaster",
                        Image="https://d1aeri3ty3izns.cloudfront.net/media/39/394972/600/preview.jpg"}
                });

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
