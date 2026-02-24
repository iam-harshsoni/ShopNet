using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopNet.Models;

namespace ShopNet.Data
{
    // Change: DbContext → IdentityDbContext<ApplicationUser>
    // This adds all 7 Identity tables automatically
    public class ShopNestDbContext : IdentityDbContext<ApplicationUser>
    {
        // Construtor - receives options (Connection string etc.) via DI
        public ShopNestDbContext(DbContextOptions<ShopNestDbContext> option) : base(option)
        {
        }

        // DBSet - one property per table
        // EF Core will create table names after these
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Table Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Slug).IsRequired().HasMaxLength(100);

                entity.HasIndex(c => c.Slug).IsUnique(); //Slug must be unique
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(p => p.Description).HasMaxLength(2000);

                entity.HasIndex(p => p.CategoryId);     // Index on CategoryId for faster JOIN queries
                entity.HasIndex(p => p.Name);           // Index on Name for faster search

                //Define Relationship: Product belong to one Category
                //One Category has many Product
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Dont delete category if product exits.
            });

            // Order → OrderItems relationship
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId);

                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // Auto-update UpdatedAt on every save
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var entries = ChangeTracker.Entries()
                        .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;

                    // Prevent accidental modification of CreatedAt
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                }
            }

            return base.SaveChangesAsync(ct);
        }
    }
}