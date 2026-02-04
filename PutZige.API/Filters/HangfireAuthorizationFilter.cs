using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace PutZige.API.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var env = httpContext.RequestServices.GetService<IHostEnvironment>();

        if (env?.IsDevelopment() ?? false)
            return true;

        // Allow only users with a specific claim in non-development environments
        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated ?? false)
        {
            return user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        return false;
    }
}
