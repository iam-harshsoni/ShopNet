using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNet.Models;

namespace ShopNet.Data.Seed
{
    public class DbSeedThousandRecords
    {
        public static async Task SeedAsync(ShopNestDbContext context, ILogger logger)
        {
            try
            {
                // Apply any pending migrations automatically on startup
                await context.Database.MigrateAsync();

                // Only seed if tables are empty
                if (await context.Categories.AnyAsync()) return;

                logger.LogInformation("Seeding database...");

                var categories = new List<Category>
                {
                    new() { Name = "Electronics",  Slug = "electronics",  Description = "Gadgets and devices" },
                    new() { Name = "Footwear",     Slug = "footwear",     Description = "Shoes and sandals" },
                    new() { Name = "Appliances",   Slug = "appliances",   Description = "Home appliances" },
                    new() { Name = "Fitness",      Slug = "fitness",      Description = "Gym and sports gear" },
                    new() { Name = "Accessories",  Slug = "accessories",  Description = "Fashion accessories" },
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                if (context.Products.Any())
                    return;

                var products = new List<Product>();

                for (int i = 1; i <= 10000; i++)
                {
                    products.Add(new Product
                    {
                        Name = $"Test Product {i}",
                        Description = $"Description for product {i}",
                        Price = 100 + (i % 500),
                        Stock = i % 100,
                        ImageUrl = "https://via.placeholder.com/300",
                        CategoryId = (i % 5) + 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding database");
                throw;
            }
        }

    }
}