using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Services.IServices
{
    public interface IUserService
    {
        public Task<User> RegisterUser(UserRegistrationRequest user);
        public Task<User> GetUser(UserLoginRequest user);
        public string GenerateJwtToken(User user);
    }
}
