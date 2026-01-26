# SignalR Real-Time Messaging - Multi-Phase Implementation

You are Anders Hejlsberg (creator of C#, TypeScript) with 25+ years building distributed systems at Microsoft.

You write production-grade code that is:
- Clean, readable, maintainable
- Following SOLID principles  
- Battle-tested patterns only
- Zero over-engineering
- Proper error handling
- Comprehensive logging
- Fully tested
- Highly optimized queries
- Zero hardcoding (use constants/settings)

No tutorials. No comments explaining basics. Write code a Distinguished Engineer would approve.

---

## Project Context

**Clean Architecture .NET 10 Chat App:**
- ✅ User registration/login with JWT (done)
- ✅ Rate limiting (done)
- ✅ Clean extension methods in Program.cs
- ⏳ **Next:** SignalR real-time messaging (text-only)

**Existing patterns:**
- Extensions in `PutZige.API/Extensions/`
- Settings in `PutZige.*/Settings/` with Options pattern
- Services registered via `Add*Services()` methods
- All config in appsettings (Dev, Staging, Test, Release)
- Entities inherit BaseEntity (soft delete, audit)

## PHASE 4: SignalR Hub & Configuration

### Your Task:

**1. Create SignalR Settings**

**File:** `PutZige.Infrastructure/Settings/SignalRSettings.cs`

```csharp
public sealed class SignalRSettings
{
    public const string SectionName = "SignalRSettings";
    
    public bool EnableRedis { get; init; } = false;
    public string? RedisConnectionString { get; init; }
    public int KeepAliveIntervalSeconds { get; init; } = 15;
    public int ClientTimeoutSeconds { get; init; } = 30;
    public int HandshakeTimeoutSeconds { get; init; } = 15;
}
```

**2. Add to all appsettings files**

**Files:** 
- appsettings.json
- appsettings.Development.json
- appsettings.Staging.json
- appsettings.Test.json
- appsettings.Release.json (if exists)

```json
"SignalRSettings": {
  "EnableRedis": false,
  "RedisConnectionString": null,
  "KeepAliveIntervalSeconds": 15,
  "ClientTimeoutSeconds": 30,
  "HandshakeTimeoutSeconds": 15
}
```

**3. Create Chat Hub**

**File:** `PutZige.API/Hubs/ChatHub.cs`

**Requirements:**
- Inherits Hub
- [Authorize] attribute (JWT auth required)
- OnConnectedAsync: Track user connection (userId → connectionId mapping)
- OnDisconnectedAsync: Clean up connection tracking
- SendMessage method: Validate, save to DB via IMessagingService, send to receiver if online
- Concise logging

**Example structure:**
```csharp
[Authorize]
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, string> UserConnections = new();
    private readonly IMessagingService _messagingService;
    private readonly ILogger<ChatHub> _logger;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromClaims();
        UserConnections[userId] = Context.ConnectionId;
        _logger?.LogInformation("User connected - UserId: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(Guid receiverId, string messageText)
    {
        var senderId = GetUserIdFromClaims();
        var response = await _messagingService.SendMessageAsync(senderId, receiverId, messageText);
        
        // Send to receiver if online
        if (UserConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", response);
            await _messagingService.MarkMessageAsDeliveredAsync(response.MessageId);
        }
        
        // Send confirmation to sender
        await Clients.Caller.SendAsync("MessageSent", response);
    }
}
```

**4. Create SignalR Extension**

**File:** `PutZige.API/Extensions/SignalRExtensions.cs`

```csharp
public static IServiceCollection AddSignalRConfiguration(this IServiceCollection services, IConfiguration configuration)
{
    var settings = configuration.GetSection(SignalRSettings.SectionName).Get<SignalRSettings>();
    
    var signalRBuilder = services.AddSignalR(options =>
    {
        options.KeepAliveInterval = TimeSpan.FromSeconds(settings?.KeepAliveIntervalSeconds ?? 15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(settings?.ClientTimeoutSeconds ?? 30);
        options.HandshakeTimeout = TimeSpan.FromSeconds(settings?.HandshakeTimeoutSeconds ?? 15);
    });

    // Add Redis backplane if enabled (optional, for multi-server scaling)
    if (settings?.EnableRedis == true && !string.IsNullOrWhiteSpace(settings.RedisConnectionString))
    {
        signalRBuilder.AddStackExchangeRedis(settings.RedisConnectionString);
        // Note: Requires Microsoft.AspNetCore.SignalR.StackExchangeRedis package (install when needed)
    }

    return services;
}

public static void MapSignalRHubs(this WebApplication app)
{
    app.MapHub<ChatHub>("/hubs/chat");
}
```

**5. Update Program.cs**

**File:** `PutZige.API/Program.cs`

Add in services section:
```csharp
builder.Services.AddSignalRConfiguration(builder.Configuration);
```

Add in middleware section (after app.UseAuthorization):
```csharp
app.MapSignalRHubs();
```

**6. Add JWT to SignalR**

**File:** `PutZige.API/Extensions/AuthenticationExtensions.cs`

Update AddJwtBearer to support SignalR:
```csharp
.AddJwtBearer(options =>
{
    // ... existing config ...
    
    // Allow JWT from query string for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});
```

**Constraints:**
- ✅ Follow existing extension pattern
- ✅ Use settings, no hardcoding
- ✅ JWT auth on Hub
- ✅ Redis-ready (works with/without Redis)
- ✅ Concise logging
- ✅ Proper error handling

**Deliverables:**
1. SignalRSettings.cs
2. All appsettings files updated
3. ChatHub.cs
4. SignalRExtensions.cs
5. Updated Program.cs
6. Updated AuthenticationExtensions.cs
7. Build project - verify no errors
8. Tell me: "Phase 4 complete. SignalR hub ready. Ready for Phase 5 (REST endpoints + tests)."