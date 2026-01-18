#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PutZige.Application.DTOs.Common
{
    /// <summary>
    /// Standard API response wrapper used across the application.
    /// </summary>
    /// <typeparam name="T">Type of the data payload.</typeparam>
    public sealed class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public bool IsSuccess { get; init; }

        /// <summary>
        /// The returned data when the operation is successful.
        /// </summary>
        public T? Data { get; init; }

        /// <summary>
        /// A human readable message intended for clients.
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Optional validation or domain errors keyed by field.
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; init; }

        /// <summary>
        /// Optional HTTP status code associated with the response.
        /// </summary>
        public int? StatusCode { get; init; }

        /// <summary>
        /// The server timestamp when the response was created (UTC).
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a successful ApiResponse containing data.
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "")
            => new()
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            };

        /// <summary>
        /// Creates an error ApiResponse with optional errors dictionary and status code.
        /// </summary>
        public static ApiResponse<T> Error(string message, Dictionary<string, string[]>? errors = null, int statusCode = 400)
            => new()
            {
                IsSuccess = false,
                Data = default,
                Message = message,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
    }
}
