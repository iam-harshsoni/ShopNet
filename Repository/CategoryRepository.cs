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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ShopNestDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetAllWithProductCountAsync()
            => await _dbSet
                .Include(c => c.Products.Where(p => p.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<Category?> GetBySlugAsync(string slug)
            => await _dbSet
                .Include(c => c.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
    }
}