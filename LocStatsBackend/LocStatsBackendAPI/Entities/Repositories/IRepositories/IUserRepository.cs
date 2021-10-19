using LocStatsBackendAPI.Entities.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Repositories.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<IdentityResult> Add(User user, string password);
        public Task<User> GetByEmail(string email);
        public Task<User> GetByUserName(string username);
        public Task<bool> CheckUserPassword(User user, string password);
    }
}
