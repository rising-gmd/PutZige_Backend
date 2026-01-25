You are a principal engineer implementing ASP.NET Core built-in rate limiting for the PutZige chat application following clean architecture principles and existing project conventions.
Solution Context:

Application Type: Real-time chat application (bursty traffic patterns)
Architecture: Clean Architecture (Domain → Infrastructure → Application → API)
Framework: .NET 10 (built-in rate limiting available via Microsoft.AspNetCore.RateLimiting)
Configuration: IOptions pattern with FluentValidation (follow existing patterns)
Environment files: appsettings.json, appsettings.Development.json, appsettings.Staging.json, appsettings.Production.json
Existing patterns:

Settings classes in Application/Settings/ or Infrastructure/Settings/
Validators in Application/Validators/
Extensions in API/Extensions/
Clean separation of concerns - DO NOT bloat Program.cs



Controllers to Protect:
AuthController.cs (api/v1/auth):

POST /login - Strict: 5 attempts per 15 min (Fixed Window)
POST /refresh-token - Moderate: 10 attempts per 15 min (Fixed Window)

UsersController.cs (api/v1/users):

POST /users (registration) - Strict: 3 registrations per hour (Fixed Window)

All Other Endpoints (chat, messages, presence, etc.):

Global: 1000 requests per minute per user (Sliding Window)
Applied via options.GlobalLimiter - secure by default

Rate Limiting Strategy:
Sliding Window for Global API (chat endpoints):

Chat has bursty traffic (user sends multiple messages quickly)
Sliding window = smoother UX, no boundary gaming
Worth 2x memory overhead for better experience

Fixed Window for Authentication (login/register):

Simple, sufficient for auth protection
Lower overhead, auth traffic not bursty

GlobalLimiter (Option C):

All endpoints protected by default (Sliding Window 1000/min)
Specific endpoints override with explicit [EnableRateLimiting] attributes
Secure by default - new endpoints automatically protected
No manual attribute management on every endpoint

Requirements:
1. Rate Limiting Configuration (IOptions + FluentValidation)
Create RateLimitSettings.cs (follow project's Settings folder location):
csharppublic class RateLimitSettings
{
    public const string SectionName = "RateLimitSettings";
    
    // Feature toggle
    public bool Enabled { get; set; } = true;
    
    // Policy configurations
    public SlidingWindowPolicySettings GlobalApi { get; set; } = new();
    public FixedWindowPolicySettings Login { get; set; } = new();
    public FixedWindowPolicySettings RefreshToken { get; set; } = new();
    public FixedWindowPolicySettings Registration { get; set; } = new();
    
    // Optional: Distributed cache for multi-server
    public bool UseDistributedCache { get; set; } = false;
    public string? RedisConnectionString { get; set; }
}

public class FixedWindowPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}

public class SlidingWindowPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
    public int SegmentsPerWindow { get; set; } = 8;
}
Create RateLimitSettingsValidator.cs (Application/Validators/):

Validate PermitLimit > 0 and <= 10000
Validate WindowSeconds > 0 and <= 86400 (24 hours)
Validate SegmentsPerWindow >= 2 and <= 100 (for sliding window)
Validate Enabled is boolean
Validate nested PolicySettings objects (both Fixed and Sliding)

Update appsettings files:
appsettings.json (production defaults):
json{
  "RateLimitSettings": {
    "Enabled": true,
    "GlobalApi": {
      "PermitLimit": 1000,
      "WindowSeconds": 60,
      "SegmentsPerWindow": 8
    },
    "Login": {
      "PermitLimit": 5,
      "WindowSeconds": 900
    },
    "RefreshToken": {
      "PermitLimit": 10,
      "WindowSeconds": 900
    },
    "Registration": {
      "PermitLimit": 3,
      "WindowSeconds": 3600
    },
    "UseDistributedCache": false,
    "RedisConnectionString": null
  }
}
appsettings.Development.json (relaxed for dev):
json{
  "RateLimitSettings": {
    "Enabled": true,
    "GlobalApi": { "PermitLimit": 10000, "WindowSeconds": 60, "SegmentsPerWindow": 8 },
    "Login": { "PermitLimit": 1000, "WindowSeconds": 60 },
    "RefreshToken": { "PermitLimit": 1000, "WindowSeconds": 60 },
    "Registration": { "PermitLimit": 100, "WindowSeconds": 60 }
  }
}
appsettings.Staging.json (moderate):
json{
  "RateLimitSettings": {
    "Enabled": true,
    "GlobalApi": { "PermitLimit": 2000, "WindowSeconds": 60, "SegmentsPerWindow": 8 },
    "Login": { "PermitLimit": 10, "WindowSeconds": 900 },
    "RefreshToken": { "PermitLimit": 20, "WindowSeconds": 900 },
    "Registration": { "PermitLimit": 5, "WindowSeconds": 3600 }
  }
}
appsettings.Production.json (use defaults from appsettings.json or override if needed)
2. Rate Limiting Service Registration (Clean Architecture)
Create RateLimitingExtensions.cs in PutZige.API/Extensions/:

Create extension method: AddRateLimitingConfiguration(this IServiceCollection services, IConfiguration configuration)
Register and validate RateLimitSettings with IOptions pattern using FluentValidation
Configure AddRateLimiter() with:

Named Policies (for specific endpoints):

"login" - Fixed window (5 requests per 15 min)
"refresh-token" - Fixed window (10 requests per 15 min)
"registration" - Fixed window (3 requests per hour)

GlobalLimiter (for all other endpoints):

Use options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(...)
Apply Sliding window limiter (1000 requests per minute)
Smart bypass: If endpoint already has [EnableRateLimiting] attribute, skip global limiter
Check for existing rate limit metadata: endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>()
If specific policy exists, return RateLimitPartition.GetNoLimiter("bypass")

Partition key strategy:

For authenticated requests: Use User ID from JWT claims (sub claim)
For unauthenticated: Use IP address from X-Forwarded-For header (if behind proxy) or RemoteIpAddress
Fallback to "unknown" if neither available
Code pattern:

csharp    var userId = context.User?.FindFirst("sub")?.Value;
    if (userId == null)
    {
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString();
        userId = ip ?? "unknown";
    }
Custom rejection response (options.OnRejected):

Set StatusCode = 429
Add Retry-After header in seconds
Return JSON with:

json    {
      "error": "Rate limit exceeded. Please try again later.",
      "retryAfter": 45,
      "policyName": "global-api",
      "limit": 1000,
      "window": 60
    }

Include structured logging (IP/UserID, endpoint, policy, limit details)

Additional requirements:

Respect Enabled setting - if false, register services but don't apply any rate limiting
Log configuration on startup (policies registered, limits, enabled/disabled status)
Handle X-Forwarded-For for reverse proxy scenarios (Cloudflare, nginx, load balancers)
Graceful degradation if settings validation fails (log error, disable rate limiting, don't crash)

DO NOT put implementation directly in Program.cs - keep it clean via extension method
3. Middleware Registration
Create or update ApplicationBuilderExtensions.cs in PutZige.API/Extensions/:

Create: UseRateLimitingMiddleware(this IApplicationBuilder app)
Call app.UseRateLimiter() - built-in middleware
CRITICAL Middleware Order:

csharp  app.UseRouting();
  app.UseAuthentication();  // MUST be before rate limiting (for User ID extraction)
  app.UseRateLimitingMiddleware(); // ← Add here
  app.UseAuthorization();
  app.MapControllers();

Add XML documentation explaining middleware order importance
Check if rate limiting is enabled before calling UseRateLimiter()

4. Apply Rate Limiting to Controllers
Update AuthController.cs (only these endpoints need explicit attributes):
csharp[HttpPost("login")]
[EnableRateLimiting("login")] // ← 5/15min
public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(...)

[HttpPost("refresh-token")]
[EnableRateLimiting("refresh-token")] // ← 10/15min
public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken(...)
Update UsersController.cs:
csharp[HttpPost]
[EnableRateLimiting("registration")] // ← 3/hour
public async Task<ActionResult<ApiResponse<RegisterUserResponse>>> CreateUser(...)
All other endpoints:

DO NOT add any attributes
GlobalLimiter automatically applies Sliding Window (1000/min)
Secure by default

5. Program.cs Integration (Keep it Clean)
In Program.cs, only add these TWO lines:
csharp// Service registration (after AddControllers, before Build)
builder.Services.AddRateLimitingConfiguration(builder.Configuration);

// Middleware pipeline (AFTER UseAuthentication, BEFORE UseAuthorization)
app.UseRateLimitingMiddleware();
Show exact placement in Program.cs - before and after context for clarity
6. Distributed Cache Support (Production Ready - Optional)
If UseDistributedCache = true in settings:

Connect to Redis using provided connection string
Use partitioned rate limiter with Redis-backed distributed cache
Redis key format: ratelimit:{policyName}:{partitionKey}
Graceful fallback to in-memory if Redis unavailable (log warning, don't crash)
Add try-catch around Redis connection with proper error handling

Redis setup notes:

Document connection string format in appsettings comments
Explain when to enable (multi-server, horizontal scaling)
Note that single-server deployments don't need Redis

7. Logging and Monitoring
Structured logging requirements:
On Startup:

Log: Rate limiting enabled/disabled
Log: Policies registered (names, limits, windows)
Log: GlobalLimiter configuration (limit, window, segments)
Log: Distributed cache status (enabled/disabled, Redis connection status)

On Rate Limit Hit (Warning level):
csharp_logger.LogWarning(
    "Rate limit exceeded: Policy={PolicyName}, Endpoint={Endpoint}, Partition={PartitionKey}, " +
    "Limit={Limit}, Window={WindowSeconds}s, Algorithm={Algorithm}",
    policyName, endpoint, partitionKey, permitLimit, windowSeconds, "SlidingWindow"
);
```

**On Configuration Error (Error level):**
- Log validation failures
- Log Redis connection failures
- Always include actionable error messages

**Never log**:
- Sensitive user data
- Full JWT tokens
- Passwords or credentials

### 8. Error Handling

**Configuration validation failure**:
- Log detailed error with which setting failed
- Disable rate limiting (set Enabled = false internally)
- Continue application startup (don't crash)
- Return warning in health check endpoint

**Redis connection failure**:
- Log warning with connection details (not password)
- Fallback to in-memory rate limiting
- Set flag for monitoring/alerts
- Retry connection in background (optional)

**Runtime errors**:
- Catch exceptions in rate limiting logic
- Log error and allow request through (fail open for availability)
- Track error count for monitoring

### 9. Security Considerations

**IP Address Handling**:
- Check `X-Forwarded-For` header first (trusted proxies only)
- Validate IP format (prevent header injection)
- Handle IPv6 addresses correctly
- Normalize IPs (remove port numbers)

**User ID Extraction**:
- Extract from `sub` claim in JWT
- Validate claim exists and is not empty
- Don't trust client-provided user IDs (only from validated JWT)

**Attack Prevention**:
- GlobalLimiter prevents DDoS on new endpoints
- Specific policies prevent brute force (login) and spam (registration)
- Sliding window prevents boundary gaming
- Distributed cache prevents cross-server abuse

### 10. Performance Considerations

**Sliding Window Trade-offs**:
- Memory: ~2x Fixed Window (8 segments × users)
- CPU: Minimal overhead per request
- **Worth it** for chat UX (smoother experience)

**Optimization tips**:
- SegmentsPerWindow = 8 is optimal (balance accuracy vs memory)
- For extreme high traffic, reduce to 4 segments
- Monitor memory usage in production
- Consider Redis for >10K concurrent users

**Benchmarking**:
- Document expected memory per 1000 users
- Provide guidance on when to scale horizontally
- Redis becomes important at ~50K+ requests/min

## Code Quality Standards:

- Follow SOLID principles
- Keep Program.cs minimal (max 2 lines for rate limiting)
- XML documentation on all public methods and classes
- Structured logging with ILogger<T> (never log sensitive data)
- Use existing project conventions exactly (Settings folders, Validators, Extensions)
- Configuration-driven (zero hardcoded values)
- Environment-specific settings (dev/staging/prod with appropriate limits)
- Feature toggle support (Enabled flag)
- Defensive coding (null checks, graceful degradation, fail-open on errors)
- Follow async/await best practices

## Deliverables:

1. **RateLimitSettings.cs** - Configuration class with Fixed and Sliding window settings
2. **RateLimitSettingsValidator.cs** - FluentValidation validator for all settings
3. **RateLimitingExtensions.cs** - Complete service registration with:
   - Named policies (login, refresh-token, registration)
   - GlobalLimiter with smart bypass logic
   - Rejection handler with detailed response
   - Distributed cache support (optional)
   - Comprehensive logging
4. **ApplicationBuilderExtensions.cs** - Middleware extension with correct order
5. **Updated AuthController.cs** - Only methods with [EnableRateLimiting] attributes
6. **Updated UsersController.cs** - Only methods with [EnableRateLimiting] attributes
7. **Updated appsettings.json** - All environments (json, Development, Staging, Production)
8. **Updated Program.cs** - Clean integration (show exact 2-line placement)
9. **Implementation Notes** - Document explaining:
   - Why Sliding Window for chat (UX benefits, boundary gaming prevention)
   - Why Fixed Window for auth (simplicity, sufficient protection)
   - Why GlobalLimiter (secure by default, zero maintenance)
   - Middleware order criticality (auth before rate limit)
   - User ID vs IP partitioning strategy
   - When to use Redis (multi-server, scaling guidance)
   - Memory implications and optimization tips
   - Monitoring and alerting recommendations
   - How to disable for testing/debugging

## Output Format:
```
## 1. CONFIGURATION (IOptions + FluentValidation)

### RateLimitSettings.cs
[complete code]

### RateLimitSettingsValidator.cs
[complete code with all validation rules]

### appsettings.json
[production config]

### appsettings.Development.json
[dev config - relaxed limits]

### appsettings.Staging.json
[staging config - moderate limits]

### appsettings.Production.json
[production overrides if any]

## 2. EXTENSIONS (Keep Program.cs Clean)

### RateLimitingExtensions.cs
[complete implementation including:
 - Service registration method
 - Named policies configuration (Fixed Window)
 - GlobalLimiter configuration (Sliding Window with smart bypass)
 - Partition key logic (User ID + IP with X-Forwarded-For)
 - Rejection handler with detailed JSON response
 - Structured logging throughout
 - Redis support (conditional based on settings)
 - Error handling and graceful degradation
 - XML documentation
]

### ApplicationBuilderExtensions.cs
[middleware registration with:
 - Correct order explanation
 - Enabled flag check
 - XML documentation
]

## 3. CONTROLLERS (Apply Specific Attributes Only)

### AuthController.cs (UPDATED)
[show only the two methods with attributes - login and refresh-token]

### UsersController.cs (UPDATED)
[show only the CreateUser method with registration attribute]

## 4. PROGRAM.CS (Minimal Integration)

### Before (existing code context):
[show surrounding lines for context]

### Add these 2 lines:
[exact code to add with comments]

### After (updated code):
[show final placement in context]

## 5. IMPLEMENTATION NOTES

### Architecture Decisions:
- Why Sliding Window for chat endpoints
- Why Fixed Window for authentication
- Why GlobalLimiter vs manual attributes
- Middleware order importance

### Security Considerations:
- User ID vs IP partitioning
- X-Forwarded-For header handling
- Attack vectors prevented

### Performance & Scaling:
- Memory implications of Sliding Window
- When to enable Redis (traffic thresholds)
- Optimization tips for high traffic

### Operational Guide:
- How to monitor rate limiting
- How to adjust limits per environment
- How to disable for testing
- How to troubleshoot issues
- Redis setup and fallback behavior

### Breaking Changes:
[any breaking changes or migration notes]
Show me the complete, production-ready implementation following PutZige's clean architecture patterns. The code must be ready to merge into production.