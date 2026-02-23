using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopNet.Models;

namespace ShopNet.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllWithProductCountAsync();
        Task<Category?> GetBySlugAsync(string slug);
    }
}