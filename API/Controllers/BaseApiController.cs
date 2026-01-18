using Microsoft.AspNetCore.Mvc;
using PutZige.Application.DTOs.Common;
using System.Collections.Generic;

namespace PutZige.API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "")
            => Ok(ApiResponse<T>.Success(data, message));

        protected ActionResult<ApiResponse<T>> Created<T>(T data, string message = "")
            => StatusCode(201, ApiResponse<T>.Success(data, message));

        protected ActionResult<ApiResponse<T>> BadRequestError<T>(string message, Dictionary<string, string[]>? errors = null)
            => BadRequest(ApiResponse<T>.Error(message, errors, 400));

        protected ActionResult<ApiResponse<T>> NotFoundError<T>(string message)
            => NotFound(ApiResponse<T>.Error(message, null, 404));

        protected ActionResult<ApiResponse<T>> UnauthorizedError<T>(string message)
            => Unauthorized(ApiResponse<T>.Error(message, null, 401));

        protected ActionResult<ApiResponse<T>> ForbiddenError<T>(string message)
            => StatusCode(403, ApiResponse<T>.Error(message, null, 403));

        protected ActionResult<ApiResponse<T>> ServerError<T>(string message)
            => StatusCode(500, ApiResponse<T>.Error(message, null, 500));
    }
}
