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
using Microsoft.AspNetCore.RateLimiting;

namespace PutZige.API.Controllers
{
    [Route("api/v1/auth")]
    public sealed class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var response = await _authService.LoginAsync(request.Identifier, request.Password, ct);
            return Ok(ApiResponse<LoginResponse>.Success(response, SuccessMessages.Authentication.LoginSuccessful));
        }

        /// <summary>
        /// Refreshes the authentication token.
        /// </summary>
        [HttpPost("refresh-token")]
        [EnableRateLimiting("refresh-token")]
        public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
            return Ok(ApiResponse<RefreshTokenResponse>.Success(response, SuccessMessages.Authentication.TokenRefreshed));
        }
    }
}
