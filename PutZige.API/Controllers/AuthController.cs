#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.DTOs.Common;
using PutZige.Application.Interfaces;
using PutZige.Domain.Entities;
using FluentValidation;
using PutZige.Application.Extensions;
using System.Collections.Generic;
using PutZige.Application.Common.Messages;

namespace PutZige.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController>? _logger;
        private readonly IValidator<RegisterUserRequest> _validator;

        public AuthController(IUserService userService, ILogger<AuthController>? logger = null, IValidator<RegisterUserRequest> validator = null!)
        {
            _userService = userService;
            _logger = logger;
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterUserResponse>>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
        {
            // Manual validation
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                var errorsDict = new Dictionary<string, string[]>(errors);

                _logger?.LogWarning("Validation failed for {RequestType} - Email: {Email}, Errors: {@Errors}", nameof(RegisterUserRequest), request.Email, errorsDict);
                return BadRequest(ApiResponse<RegisterUserResponse>.Error(
                    ErrorMessages.Validation.ValidationFailed,
                    errorsDict,
                    400));
            }

            _logger?.LogInformation("User registration attempt - Email: {Email}", request.Email);

            var user = await _userService.RegisterUserAsync(request.Email, request.Username, request.DisplayName, request.Password, ct);

            var response = new RegisterUserResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                DisplayName = user.DisplayName,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt
            };

            _logger?.LogInformation("User registration successful - UserId: {UserId}, Email: {Email}", user.Id, user.Email);

            return CreatedAtAction(nameof(Register), new { id = user.Id }, ApiResponse<RegisterUserResponse>.Success(response, SuccessMessages.Authentication.RegistrationSuccessful));
        }
    }
}
