using LocStatsBackendAPI.Data;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Repositories.IRepositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Repositories
{
    public class UserRepository : IUserRepository
    {
        protected readonly UserManager<User> UserManager;
        protected AppDbContext Context;

        public UserRepository(UserManager<User> userManager, AppDbContext context)
        {
            UserManager = userManager;
            Context = context;
        }

        public IQueryable<User> GetAll()
        {
            return UserManager.Users;
        }

        public async Task<User> GetById(Guid id)
        {
            return await UserManager.FindByIdAsync(id.ToString());
        }

        public IQueryable<User> Find(Expression<Func<User, bool>> where)
        {
            return UserManager.Users.AsQueryable().Where(where);
        }

        public Task<bool> Add(User entity)
        {
            throw new NotSupportedException();
        }

        public async Task<IdentityResult> Add(User user, string password)
        {
            return await UserManager.CreateAsync(user, password);
        }

        public async Task<bool> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetByEmail(string email)
        {
            return await UserManager.FindByEmailAsync(email);
        }

        public async Task<User> GetByUserName(string username)
        {
            return await UserManager.FindByNameAsync(username);
        }

        public async Task<bool> CheckUserPassword(User user, string password)
        {
            return await UserManager.CheckPasswordAsync(user, password);
        }
    }
}
