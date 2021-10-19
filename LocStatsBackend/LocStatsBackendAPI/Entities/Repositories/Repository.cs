using LocStatsBackendAPI.Data;
using LocStatsBackendAPI.Entities.Repositories.IRepositories;
using log4net.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected DbSet<T> DbSet;
        protected AppDbContext Context;

        public Repository(AppDbContext context)
        {
            Context = context;
            DbSet = Context.Set<T>();
        }

        public virtual async Task<bool> Add(T entity)
        {
            await DbSet.AddAsync(entity);
            return true;
        }

        public virtual IQueryable<T> GetAll()
        {
            return DbSet.AsQueryable();
        }

        public virtual IQueryable<T> Find(Expression<Func<T, bool>> where)
        {
            return DbSet.AsQueryable().Where(where);
        }

        public virtual async Task<T> GetById(Guid id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<bool> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
