using LocStatsBackendAPI.Data;
using LocStatsBackendAPI.Entities.Exceptions;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Repositories.IRepositories;
using LocStatsBackendAPI.Entities.Requests;
using LocStatsBackendAPI.Services.IServices;
using log4net.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Services
{
    public class UserService : IUserService
    {
        protected readonly JwtConfig JwtConfig;
        protected readonly IUserRepository UserRepository;

        public UserService(IOptionsMonitor<JwtConfig> jwtConfig, IUserRepository userRepository)
        {
            JwtConfig = jwtConfig.CurrentValue;
            UserRepository = userRepository;
        }

        public async Task<User> RegisterUser(UserRegistrationRequest user)
        {

            var userWithSameEmail = await UserRepository.GetByEmail(user.Email);

            if (userWithSameEmail != null)
            {
                throw new AuthException("Email is already in use");
            }

            var userWithSameName = await UserRepository.GetByUserName(user.UserName);

            if (userWithSameName != null)
            {
                throw new AuthException("Username is already in use");
            }

            var newUser = new User { Email = user.Email, UserName = user.UserName };
            var isCreated = await UserRepository.Add(newUser, user.Password);

            if (!isCreated.Succeeded)
            {
                throw new AuthException("Username could not be created", isCreated.Errors.Select(x => x.Description));
            }

            return newUser;
        }

        public async Task<User> GetUser(UserLoginRequest user)
        {
            var foundUser = await UserRepository.GetByUserName(user.UserName);

            if (foundUser == null)
            {
                throw new AuthException("Invalid username and/or password");
            }

            var isPasswordValid = await UserRepository.CheckUserPassword(foundUser, user.Password);

            if (!isPasswordValid)
            {
                throw new AuthException("Invalid username and/or password");
            }

            return foundUser;
        }


        public string GenerateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(1), //TODO: make it smaller after implementing refresh token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
