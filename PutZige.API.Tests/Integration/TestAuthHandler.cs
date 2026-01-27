#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PutZige.API.Tests.Integration;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Get token from Authorization header
        var auth = Request.Headers.Authorization.ToString();

        // Fallback: Check query string for SignalR WebSocket connections
        if (string.IsNullOrWhiteSpace(auth))
        {
            var qs = Request.Query["access_token"].ToString();
            if (!string.IsNullOrWhiteSpace(qs))
            {
                auth = "Bearer " + qs;
            }
        }

        if (string.IsNullOrWhiteSpace(auth))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (auth.StartsWith("Bearer "))
        {
            var token = auth.Substring("Bearer ".Length).Trim();
            var claimsList = new System.Collections.Generic.List<Claim>();

            try
            {
                // Try parsing as JWT first
                if (token.Contains('.'))
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    var sub = jwt.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                    var email = jwt.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
                    var username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;

                    if (!string.IsNullOrWhiteSpace(sub))
                    {
                        claimsList.Add(new Claim("sub", sub));
                        claimsList.Add(new Claim(ClaimTypes.NameIdentifier, sub)); // Add this for compatibility
                    }
                    if (!string.IsNullOrWhiteSpace(email))
                        claimsList.Add(new Claim("email", email));
                    if (!string.IsNullOrWhiteSpace(username))
                        claimsList.Add(new Claim("username", username));
                }
                else
                {
                    // Token is plain GUID (for test scenarios)
                    if (Guid.TryParse(token, out var userId))
                    {
                        claimsList.Add(new Claim("sub", userId.ToString()));
                        claimsList.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                        claimsList.Add(new Claim(ClaimTypes.Name, $"TestUser_{userId}"));
                    }
                    else
                    {
                        // Fallback: produce a deterministic GUID from the raw token so tests using
                        // non-GUID tokens still yield a stable, parseable user id across requests.
                        var tokenBytes = Encoding.UTF8.GetBytes(token ?? string.Empty);
                        byte[] hashBytes;
                        using (var md5 = MD5.Create())
                        {
                            hashBytes = md5.ComputeHash(tokenBytes);
                        }

                        // Ensure we have 16 bytes for a GUID (MD5 produces 16 bytes)
                        var derivedId = new Guid(hashBytes);

                        claimsList.Add(new Claim("sub", derivedId.ToString()));
                        claimsList.Add(new Claim(ClaimTypes.NameIdentifier, derivedId.ToString()));
                        claimsList.Add(new Claim(ClaimTypes.Name, $"TestUser_{derivedId}"));
                    }
                }
            }
            catch
            {
                // Final fallback: treat token as raw user id
                claimsList.Add(new Claim("sub", token));
                claimsList.Add(new Claim(ClaimTypes.NameIdentifier, token));
            }

            if (claimsList.Count == 0)
                return Task.FromResult(AuthenticateResult.Fail("No valid claims found in token"));

            var identity = new ClaimsIdentity(claimsList, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }
}