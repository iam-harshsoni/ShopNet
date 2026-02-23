using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShopNet.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // Read
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        //Write
        Task AddAsync(T Entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        void Remove(T Entity);
        void RemoveRange(IEnumerable<T> entities);

        // Will move update operation to specifice Model Repo as update opertaion has different fields as per model. 
    }
}