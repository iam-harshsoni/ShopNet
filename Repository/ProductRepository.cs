using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNet.Data;
using ShopNet.Models;
using ShopNet.Repository.IRepository;

namespace ShopNet.Repository
{
        public class ProductRepository : Repository<Product>, IProductRepository
        {
                public ProductRepository(ShopNestDbContext context) : base(context) { }

                public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
                => await _dbSet
                            .Include(p => p.Category)
                            .Where(p => p.IsActive)
                            .OrderBy(p => p.Name)
                            .ToListAsync();

                public async Task<Product?> GetByIdWithCategoryAsync(int id)
                => await _dbSet
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);


                public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
                => await _dbSet
                        .Include(p => p.Category)
                        .Where(p => p.CategoryId == categoryId)
                        .OrderBy(p => p.Name)
                        .ToListAsync();

                public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
                {
                        var term = searchTerm.Trim().ToLower();
                        var productList = _dbSet
                                    .Include(p => p.Category)
                                    .Where(p => p.IsActive && (
                                        p.Name.ToLower().Contains(term) ||
                                        p.Description.ToLower().Contains(term) ||
                                        p.Category.Name.ToLower().Contains(term)
                                    ))
                                    .OrderBy(p => p.Name)
                                    .ToListAsync();

                        return await productList;
                }

                public async Task<IEnumerable<Product>> GetFeaturedProductAsync(int count = 0)
                => await _dbSet
                        .Include(p => p.Category)
                        .Where(p => p.IsActive && p.Stock > 0)
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(count)
                        .ToListAsync();

                public async Task<IEnumerable<Product>> GetInStockProductAsync()
                => await _dbSet
                        .Include(p => p.Category)
                        .Where(p => p.IsActive && p.Stock > 0)
                        .ToListAsync();

                public void Update(Product obj)
                {
                        var objFromDb = _dbSet.FirstOrDefault(x => x.Id == obj.Id);

                        if (objFromDb != null)
                        {
                                objFromDb.Name = obj.Name;
                                objFromDb.Description = obj.Description;
                                objFromDb.Price = obj.Price;
                                objFromDb.Stock = obj.Stock;
                                objFromDb.CategoryId = obj.CategoryId;

                                if (obj.ImageUrl != null)
                                        objFromDb.ImageUrl = obj.ImageUrl;
                        }
                }
        }
}