#nullable enable
using System;
using Microsoft.AspNetCore.Http;
using PutZige.Application.Interfaces;

namespace PutZige.Infrastructure.Services
{
    public sealed class ClientInfoService : IClientInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientInfoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string? GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Prefer X-Forwarded-For when behind proxies/load balancers
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var xff))
            {
                var header = xff.ToString();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    // X-Forwarded-For can contain a comma separated list
                    var parts = header.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                        return parts[0].Trim();
                }
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }

        public string? GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request?.Headers["User-Agent"].ToString();
        }

        public string? GetAcceptLanguage()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request?.Headers["Accept-Language"].ToString();
        }

        public string? GetRequestId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try TraceIdentifier then header
            if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
                return context.TraceIdentifier;

            if (context.Request.Headers.TryGetValue("X-Request-Id", out var rid))
                return rid.ToString();

            return null;
        }
    }
}
