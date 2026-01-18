#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PutZige.Application.DTOs.Common;
using PutZige.Application.Common.Messages;

namespace PutZige.API.Middleware
{
    /// <summary>
    /// Global exception handler middleware to map exceptions to HTTP responses.
    /// </summary>
    public sealed class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (status, message, errors) = MapException(ex, context);

                if ((int)status >= 500)
                {
                    _logger.LogError(ex, "Unhandled exception occurred - Type: {ExceptionType}, Message: {Message}", ex.GetType().Name, ex.Message);
                }
                else
                {
                    _logger.LogWarning(ex, "Client error - Type: {ExceptionType}, Message: {Message}, Path: {Path}", ex.GetType().Name, ex.Message, context.Request.Path);
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)status;

                var response = ApiResponse<object>.Error(message, errors, (int)status);
                var json = JsonSerializer.Serialize(response, _jsonOptions);
                await context.Response.WriteAsync(json);
            }
        }

        private static (HttpStatusCode status, string message, Dictionary<string, string[]>? errors) MapException(Exception ex, HttpContext context)
        {
            return ex switch
            {
                ValidationException ve => (HttpStatusCode.BadRequest, ErrorMessages.Validation.ValidationFailed, BuildValidationErrors(ve)),
                InvalidOperationException ioe => (HttpStatusCode.BadRequest, ioe.Message, null),
                KeyNotFoundException knf => (HttpStatusCode.NotFound, ErrorMessages.General.ResourceNotFound, null),
                UnauthorizedAccessException una => (HttpStatusCode.Unauthorized, ErrorMessages.General.UnauthorizedAccess, null),
                ArgumentNullException an => (HttpStatusCode.BadRequest, an.Message, null),
                ArgumentException ae => (HttpStatusCode.BadRequest, ae.Message, null),
                _ => (HttpStatusCode.InternalServerError, ErrorMessages.General.InternalServerError, null)
            };
        }

        private static Dictionary<string, string[]>? BuildValidationErrors(ValidationException ve)
        {
            if (ve.Errors == null) return null;

            var dict = new Dictionary<string, List<string>>();

            foreach (var error in ve.Errors)
            {
                var key = string.IsNullOrWhiteSpace(error.PropertyName) ? "" : error.PropertyName;
                if (!dict.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    dict[key] = list;
                }

                list.Add(error.ErrorMessage ?? string.Empty);
            }

            var result = new Dictionary<string, string[]>();
            foreach (var kv in dict)
            {
                result[kv.Key] = kv.Value.ToArray();
            }

            return result.Count == 0 ? null : result;
        }
    }
}
