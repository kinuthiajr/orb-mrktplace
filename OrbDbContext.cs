using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Orb.API.Models;

namespace Orb.API
{
    public class OrbDbContext : IdentityDbContext<ApplicationUser>
    {
        public OrbDbContext(DbContextOptions<OrbDbContext> options) 
        : base(options)
        { 
        }

        public DbSet<Product> Products {get; set;}
        public DbSet<Order> Orders {get; set;}
        public DbSet<OrderItem> OrderItems {get; set;}
        public DbSet<Shop> Shops {get; set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Shop>()
                .HasOne(s => s.Seller)
                .WithOne(u => u.Shop)
                .HasForeignKey<Shop>(s => s.SellerId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Product>()
                .HasOne(p => p.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Seller)
                .WithMany()
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

        
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            
        }
    }
}