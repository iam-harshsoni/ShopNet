using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopNet.Models;

namespace ShopNet.Services.Interfaces
{
    public interface IProductService
    {
        //Read 
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 0);
        Task<IEnumerable<Product>> SearchProductsAsync(string? search, int? categotyId);
        Task<Product?> GetProductByIdAsync(int id);

        //Write
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int qty);

    }
}