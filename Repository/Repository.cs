using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNet.Data;
using ShopNet.Repository.IRepository;

namespace ShopNet.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ShopNestDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ShopNestDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); // Gets the DbSet<T> for this entity type
        }

        public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        => await _dbSet.Where(filter).ToListAsync();

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        => await _dbSet.Where(filter).FirstOrDefaultAsync();

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        => await _dbSet.AnyAsync(filter);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        => filter == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(filter);

        public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities)
        => await _dbSet.AddRangeAsync(entities);

        public void Remove(T entity) => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    }
}