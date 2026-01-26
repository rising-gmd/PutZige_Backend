using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PutZige.Application.DTOs.Common;
using System;
using System.Security.Claims;
using System.Collections.Generic;

namespace PutZige.API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "")
            => Ok(ApiResponse<T>.Success(data, message));

        protected ActionResult<ApiResponse<T>> Created<T>(T data, string message = "")
            => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Success(data, message));

        protected ActionResult<ApiResponse<T>> BadRequestError<T>(string message, Dictionary<string, string[]>? errors = null)
            => BadRequest(ApiResponse<T>.Error(message, errors, StatusCodes.Status400BadRequest));

        protected ActionResult<ApiResponse<T>> NotFoundError<T>(string message)
            => NotFound(ApiResponse<T>.Error(message, null, StatusCodes.Status404NotFound));

        protected ActionResult<ApiResponse<T>> UnauthorizedError<T>(string message)
            => Unauthorized(ApiResponse<T>.Error(message, null, StatusCodes.Status401Unauthorized));

        protected ActionResult<ApiResponse<T>> ForbiddenError<T>(string message)
            => StatusCode(StatusCodes.Status403Forbidden, ApiResponse<T>.Error(message, null, StatusCodes.Status403Forbidden));

        protected ActionResult<ApiResponse<T>> ServerError<T>(string message)
            => StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<T>.Error(message, null, StatusCodes.Status500InternalServerError));

        /// <summary>
        /// Extracts the authenticated user's ID from JWT claims.
        /// </summary>
        /// <returns>Guid user id</returns>
        /// <exception cref="UnauthorizedAccessException">If the user id claim is missing or invalid.</exception>
        protected Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            return userId;
        }
    }
}
