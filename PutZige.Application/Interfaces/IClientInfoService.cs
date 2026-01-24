#nullable enable
using System;

namespace PutZige.Application.Interfaces
{
    /// <summary>
    /// Provides HTTP request metadata such as IP address, user agent, accept-language and request id.
    /// </summary>
    public interface IClientInfoService
    {
        string? GetIpAddress();
        string? GetUserAgent();
        string? GetAcceptLanguage();
        string? GetRequestId();
    }
}
