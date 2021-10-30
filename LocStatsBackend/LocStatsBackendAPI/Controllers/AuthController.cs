using AutoMapper;
using LocStatsBackendAPI.Entities.Exceptions;
using LocStatsBackendAPI.Entities.Requests;
using LocStatsBackendAPI.Entities.Responses;
using LocStatsBackendAPI.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public AuthController(ILogger<AuthController> logger, IUserService userService, IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Adds new unique user to database
        /// </summary>
        /// <param name="user">User to be added</param>
        /// <returns>Registration result</returns>
        /// <response code="201">Returns registration confirmation along with token and refresh token</response>
        /// <response code="400">Bad request</response>
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest user)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(new AuthErrorResponse("Invalid data provided"));
            }

            try
            {
                var newUser = await _userService.RegisterUser(user);
                var jwtToken = await _userService.GenerateJwtToken(newUser);

                return Ok(jwtToken);
            }
            catch (AuthException exception)
            {
                return new BadRequestObjectResult(new AuthErrorResponse(exception.Errors));
            }
            catch (Exception exception)
            {
                return new BadRequestObjectResult(new AuthErrorResponse(exception.Message));
            }
        }

        /// <summary>
        /// Returns JWT for authenticated user
        /// </summary>
        /// <param name="user">User to be logged</param>
        /// <returns>Login result</returns>
        /// <response code="200">Returns login confirmation along with token and refresh token</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(new AuthErrorResponse("Invalid data provided"));
            }

            try
            {
                var foundUser = await _userService.GetUser(user);
                var jwtToken = await _userService.GenerateJwtToken(foundUser);

                return Ok(jwtToken);
            }
            catch (AuthException exception)
            {
                return Unauthorized(new AuthErrorResponse(exception.Errors));
            }
            catch (Exception exception)
            {
                return new BadRequestObjectResult(new AuthErrorResponse(exception.Message));
            }
        }

        /// <summary>
        /// Refreshes JWT token using refresh token
        /// </summary>
        /// <param name="tokenRequest">Token and refresh token</param>
        /// <returns>Token refresh result</returns>
        /// <response code="200">Returns new refresh token and brand new refreshed JWT token</response>
        /// <response code="400">Bad request</response>
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(new AuthErrorResponse("Invalid payload"));
            
            try
            {
                var result = await _userService.VerifyAndGenerateToken(tokenRequest);
                return Ok(result);
            }
            catch (AuthException exception)
            {
                return new BadRequestObjectResult(new AuthErrorResponse(exception.Errors));
            }
        }
    }
}
