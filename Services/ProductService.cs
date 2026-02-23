using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.VisualBasic;
using ShopNet.Models;
using ShopNet.Repository.IRepository;
using ShopNet.Services.Interfaces;

namespace ShopNet.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        => await _unitOfWork.Products.GetAllWithCategoryAsync();

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 0)
        => await _unitOfWork.Products.GetFeaturedProductAsync(count);

        public async Task<IEnumerable<Product>> SearchProductsAsync(string? search, int? categotyId)
        {
            //Both filters applied
            if (!string.IsNullOrWhiteSpace(search) && categotyId.HasValue)
            {
                var searchResult = await _unitOfWork.Products.SearchAsync(search);
                return searchResult.Where(p => p.CategoryId == categotyId);
            }

            //only search
            if (!string.IsNullOrWhiteSpace(search))
                return await _unitOfWork.Products.SearchAsync(search);

            // only category
            if (categotyId.HasValue)
                return await _unitOfWork.Products.GetByCategoryAsync(categotyId.Value);

            // No filters — return all
            return await _unitOfWork.Products.GetAllWithCategoryAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        => await _unitOfWork.Products.GetByIdWithCategoryAsync(id);

        public async Task<Product> CreateProductAsync(Product product)
        {
            //Business rule: normalize name
            product.Name = product.Name.Trim();
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Products.AddAsync(product);
            Console.WriteLine(product.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product created: {ProductName} (ID: {Id})",
               product.Name, product.Id);

            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product updated: {ProductName} (ID: {Id})",
                product.Name, product.Id);
            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return false;

            //Soft delete - dont actually remove from DB
            // This preserves order history that reference this product
            product.IsActive = false;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product soft-deleted: ID {Id}", id);
            return true;
        }

        public async Task<bool> UpdateStockAsync(int productId, int qty)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return false;

            // Business rule: Stock cant be go below zero
            if (product.Stock + qty < 0)
            {
                _logger.LogWarning(
                    "Stock update rejected for Product {Id}: would go negative", productId);
                return false;
            }

            product.Stock += qty;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }


    }
}