using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> Find(Expression<Func<T, bool>> where);
        Task<T> GetById(Guid id);
        Task<bool> Add(T entity);
        Task<bool> Delete(Guid id);
        Task<bool> Update(T entity);
    }
}
