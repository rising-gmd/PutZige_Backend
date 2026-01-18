#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.DTOs.Common;
using PutZige.Application.Interfaces;
using PutZige.Application.Common.Messages;

namespace PutZige.API.Controllers
{
    [Route("api/v1/auth")]
    public sealed class AuthController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterUserResponse>>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
        {
            _logger.LogInformation("User registration attempt - Email: {Email}", request.Email);

            var response = await _userService.RegisterUserAsync(request.Email, request.Username, request.DisplayName, request.Password, ct);

            _logger.LogInformation("User registered successfully - UserId: {UserId}", response.UserId);

            return Created(response, SuccessMessages.Authentication.RegistrationSuccessful);
        }
    }
}
