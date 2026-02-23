using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopNet.Data;
using ShopNet.Repository.IRepository;

namespace ShopNet.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ShopNestDbContext _context;

        // Lazy initialization — only create repository if it's actually used
        private IProductRepository? _products;
        private ICategoryRepository? _categories;

        public UnitOfWork(ShopNestDbContext context)
        {
            _context = context;
        }

        public IProductRepository Products
         => _products ??= new ProductRepository(_context);

        public ICategoryRepository Categories
        => _categories ??= new CategoryRepository(_context);

        /* ??= is called the null-coalescing assignment operator. Assign the value only if the variable is null.

       Equivalent to; 
       if (_products == null)
       {
           _products = new ProductRepository(_context);
       }
        */

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();

    }
}