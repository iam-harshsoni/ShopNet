using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopNet.Models;

namespace ShopNet.Data.Seed
{
    public class DbSeeder
    {
        public static async Task SeedAsync(
            ShopNestDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            try
            {
                // Apply any pending migrations automatically on startup
                await context.Database.MigrateAsync();

                await SeedRolesAsync(roleManager, logger);
                await SeedAdminUserAsync(userManager, logger);
                await SeedCategoriesAndProductsAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding database");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            Console.WriteLine(">>> SeedRolesAsync started");

            string[] roles = ["Admin", "Customer", "StoreManager"];

            foreach (var role in roles)
            {
                Console.WriteLine($">>> Checking role: {role}");

                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Roles Create: {Role}", role);
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            Console.WriteLine(">>> SeedAdminUserAsync started");

            const string adminEmail = "admin@shopnet.com";

            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing != null)
            {
                Console.WriteLine(">>> Admin already exists, skipping");
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Shop",
                LastName = "Admin",
                EmailConfirmed = true,  // Skip email confirmation for seeded admin
                IsActive = true
            };

            // Use a strong password — in production, load from environment variable
            var result = await userManager.CreateAsync(admin, "Admin@12345@");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Admin user seeded: {Email}", adminEmail);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to created admin: {Errors}", errors);
            }
        }

        private static async Task SeedCategoriesAndProductsAsync(ShopNestDbContext context, ILogger logger)
        {
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

            var products = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Wireless Headphones",
                        Description = "Premium noise-cancelling headphones with 30hr battery life.",
                        Price = 2999.99m,
                        Stock = 15,
                        ImageUrl = "https://tse4.mm.bing.net/th/id/OIP.1md4aeduPt7zaJXhJlbnfgHaGQ?w=860&h=727&rs=1&pid=ImgDetMain&o=7&rm=3",
                        CategoryId =  categories[0].Id,
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Running Shoes",
                        Description = "Lightweight performance running shoes with air cushion sole.",
                        Price = 4999.99m,
                        Stock = 8,
                        ImageUrl = "https://www.campusshoes.com/cdn/shop/files/VESPER_VESPER_WHT-MILKY_BLU_07.webp?v=1758174807",
                        CategoryId = categories[0].Id,
                    },
                    new Product
                    {
                        Id = 3,
                        Name = "Coffee Maker",
                        Description = "12-cup programmable coffee maker with thermal carafe.",
                        Price = 3499.99m,
                        Stock = 0,
                        ImageUrl = "https://www.ikea.com/in/en/images/products/upphetta-coffee-tea-maker-glass-stainless-steel__0900314_pe607788_s5.jpg?f=s",
                        CategoryId = categories[1].Id,
                    },
                    new Product
                    {
                        Id = 4,
                        Name = "Mechanical Keyboard",
                        Description = "TKL mechanical keyboard with RGB and Cherry MX switches.",
                        Price = 5999.99m,
                        Stock = 22,
                        ImageUrl = "https://m.media-amazon.com/images/I/61P7MvyRbUL._AC_UF1000,1000_QL80_.jpg",
                        CategoryId = categories[2].Id,
                    },
                    new Product
                    {
                        Id = 5,
                        Name = "Yoga Mat",
                        Description = "Non-slip premium yoga mat, 6mm thick with carry strap.",
                        Price = 1299.99m,
                        Stock = 35,
                        ImageUrl = "https://sppartos.com/cdn/shop/files/31VX-aIlgWL_580x.jpg?v=1702469142",
                        CategoryId = categories[3].Id,
                    },
                    new Product
                    {
                        Id = 6,
                        Name = "Leather Wallet",
                        Description = "Slim genuine leather bifold wallet with RFID blocking.",
                        Price = 899.99m,
                        Stock = 50,
                        ImageUrl = "https://craftandglory.in/cdn/shop/products/DSC07927_1.jpg?v=1660802328&width=1946",
                        CategoryId = categories[4].Id,
                    }
                };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully.");
        }
    }
}