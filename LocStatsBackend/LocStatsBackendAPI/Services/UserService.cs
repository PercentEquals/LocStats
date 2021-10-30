using LocStatsBackendAPI.Data;
using LocStatsBackendAPI.Entities.Exceptions;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Repositories.IRepositories;
using LocStatsBackendAPI.Entities.Requests;
using LocStatsBackendAPI.Entities.Responses;
using LocStatsBackendAPI.Services.IServices;
using log4net.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        protected readonly AppDbContext Context;
        protected readonly TokenValidationParameters TokenValidationParameters;

        public UserService(IOptionsMonitor<JwtConfig> jwtConfig,
                        IUserRepository userRepository,
                        AppDbContext context,
                        TokenValidationParameters tokenValidationParams)
        {
            JwtConfig = jwtConfig.CurrentValue;
            UserRepository = userRepository;
            Context = context;
            TokenValidationParameters = tokenValidationParams;
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


        public async Task<AuthResult> GenerateJwtToken(User user)
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
                Expires = DateTime.UtcNow.AddMinutes(1).ToLocalTime(),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow.ToLocalTime(),
                ExpiryDate = DateTime.UtcNow.AddDays(200).ToLocalTime(),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await Context.RefreshTokens.AddAsync(refreshToken);
            await Context.SaveChangesAsync();

            return new AuthSuccessResponse()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, TokenValidationParameters,
                    out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512,
                        StringComparison.InvariantCulture);
                    if (result == false)
                    {
                        return null;
                    }
                }

                var utcExpiryDate = long.Parse(tokenInVerification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.UtcNow.ToLocalTime())
                    throw new AuthException("JWT token has not expired yet");

                var storedToken =
                    await Context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if (storedToken == null) throw new AuthException("Refresh token does not exist");
                if (storedToken.IsUsed) throw new AuthException("Refresh token has been used");
                if (storedToken.IsRevoked) throw new AuthException("Refresh token has been revoked");

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti) throw new AuthException("Token does not match");

                // update stored token
                storedToken.IsUsed = true;
                Context.RefreshTokens.Update(storedToken);
                await Context.SaveChangesAsync();

                // Generate a new token
                var dbUser = await UserRepository.GetById(Guid.Parse(storedToken.UserId));
                return await GenerateJwtToken(dbUser);
            }
            catch (ArgumentException exception)
            {
                throw new AuthException("JWT token has wrong format");
            }
            catch (SecurityTokenInvalidSignatureException exception)
            {
                throw new AuthException("JWT token does not exist");
            }
            catch (Exception exception)
            {
                throw new AuthException(exception.Message);
            }
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTimeVal;
        }

        private static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(x => x[random.Next(x.Length)]).ToArray());
        }
    }
}
