# AI Prompt: Implement User Login with JWT (Principal Engineer Standard)

You are a Principal Engineer at Microsoft. Implement a complete, production-ready JWT-based login feature for a chat application following existing patterns, best practices, and including comprehensive tests.

---

## Mission

Implement **POST /api/v1/auth/login** endpoint with:
- JWT access token (15 min expiry)
- Refresh token (7 days expiry, stored in DB)
- Account lockout after failed attempts
- Comprehensive logging
- Full test coverage
- Documentation updates

---

## Step 1: Analyze Existing Patterns

**Before writing ANY code, study these files:**

### Registration Pattern (Your Template):
- ✅ `PutZige.API/Controllers/AuthController.cs` - Controller pattern
- ✅ `PutZige.Application/Services/UserService.cs` - Service pattern
- ✅ `PutZige.Application/DTOs/Auth/RegisterUserRequest.cs` - Request DTO
- ✅ `PutZige.Application/DTOs/Auth/RegisterUserResponse.cs` - Response DTO
- ✅ `PutZige.Application/Validators/RegisterUserRequestValidator.cs` - Validation pattern
- ✅ `PutZige.Application/Mappings/` - AutoMapper profiles
- ✅ `PutZige.Infrastructure/Repositories/UserRepository.cs` - Repository pattern
- ✅ `PutZige.Application.Tests/Services/UserServiceTests.cs` - Testing pattern

**Output:** List the patterns you'll follow (e.g., "Use FluentValidation for request validation", "Return ApiResponse<T>", etc.)

---

## Step 2: Design the Login Feature

### Authentication Flow:
```
1. User sends email + password
2. Validate credentials (BCrypt.Verify)
3. Check account status (IsActive, IsLocked, IsEmailVerified)
4. Generate JWT access token (15 min)
5. Generate refresh token (7 day, store in UserSession table)
6. Update LastLoginAt, LastLoginIp, reset FailedLoginAttempts
7. Return both tokens + user info
```

### Account Lockout Logic:
```
- Max 5 failed attempts
- Lock account for 15 minutes
- Reset counter on successful login
- Use User.FailedLoginAttempts, User.IsLocked, User.LockedUntil
```

### Required Components:

**Domain Layer (PutZige.Domain):**
- No changes needed (User entity already has all fields)

**Application Layer (PutZige.Application):**
- `DTOs/Auth/LoginRequest.cs` - Email, Password
- `DTOs/Auth/LoginResponse.cs` - AccessToken, RefreshToken, User info, ExpiresIn
- `DTOs/Auth/RefreshTokenRequest.cs` - RefreshToken
- `DTOs/Auth/RefreshTokenResponse.cs` - New AccessToken, RefreshToken
- `Validators/LoginRequestValidator.cs` - Email/password validation
- `Services/IAuthService.cs` - Interface for auth operations
- `Services/AuthService.cs` - Login, GenerateTokens, RefreshToken logic
- `Mappings/AuthMappingProfile.cs` - AutoMapper for User → LoginResponse
- Update `Interfaces/IUserService.cs` - Add UpdateLoginInfo method

**Infrastructure Layer (PutZige.Infrastructure):**
- `Services/JwtTokenService.cs` - Generate/validate JWT tokens
- `Settings/JwtSettings.cs` - JWT configuration (Secret, Issuer, Audience, ExpiryMinutes)
- Update `Repositories/UserRepository.cs` - Add GetByEmailWithSessionAsync
- Update `DependencyInjection.cs` - Register JwtTokenService, JwtSettings

**API Layer (PutZige.API):**
- Update `Controllers/AuthController.cs` - Add Login, RefreshToken endpoints
- Update `appsettings.json` - Add JwtSettings section
- Update `Program.cs` - Add JWT authentication middleware

**Tests:**
- `PutZige.Application.Tests/Services/AuthServiceTests.cs` - Unit tests
- `PutZige.API.Tests/Controllers/AuthControllerTests.cs` - Integration tests

---

## Step 3: Implementation Requirements

### Follow These Patterns:

**✅ Validation (Use FluentValidation):**
```csharp
// Pattern from RegisterUserRequestValidator
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required")
    .EmailAddress().WithMessage("Invalid email format")
    .WithName("email");

RuleFor(x => x.Password)
    .NotEmpty().WithMessage("Password is required")
    .WithName("password");
```

**✅ Service Layer (Follow UserService pattern):**
```csharp
// Constructor with null checks
public AuthService(IUserRepository userRepo, IUnitOfWork unitOfWork, IJwtTokenService jwtService, IMapper mapper, ILogger<AuthService>? logger = null)
{
    ArgumentNullException.ThrowIfNull(userRepo);
    // ... throw for each required dependency
}

// Method structure
public async Task<LoginResponse> LoginAsync(string email, string password, string? ipAddress, CancellationToken ct = default)
{
    // 1. Validate inputs
    if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(...);
    
    // 2. Log operation start
    _logger?.LogInformation("Login attempt - Email: {Email}", email);
    
    // 3. Business logic
    // 4. Log important steps (one-liners)
    // 5. Return response
}
```

**✅ Logging (Concise, one-liner format):**
```csharp
_logger?.LogInformation("Login attempt - Email: {Email}", email);
_logger?.LogWarning("Login failed - Invalid credentials: {Email}", email);
_logger?.LogWarning("Login failed - Account locked: {Email}", email);
_logger?.LogInformation("Login successful - UserId: {UserId}", userId);
```

**✅ Error Messages (Use existing pattern):**
```csharp
// Add to PutZige.Application/Common/Messages/ErrorMessages.cs
public static class Authentication
{
    public const string InvalidCredentials = "Invalid email or password";
    public const string AccountLocked = "Account is locked. Try again later";
    public const string AccountInactive = "Account is not active";
    public const string EmailNotVerified = "Email is not verified";
    public const string InvalidRefreshToken = "Invalid or expired refresh token";
}
```

**✅ Controller (Follow existing pattern):**
```csharp
[HttpPost("login")]
public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
{
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
    var response = await _authService.LoginAsync(request.Email, request.Password, ipAddress, ct);
    return Ok(ApiResponse<LoginResponse>.Success(response, "Login successful"));
}
```

**✅ JWT Configuration (appsettings.json):**
```json
"JwtSettings": {
  "Secret": "your-256-bit-secret-key-minimum-32-characters-long",
  "Issuer": "PutZige",
  "Audience": "PutZige.Users",
  "AccessTokenExpiryMinutes": 15,
  "RefreshTokenExpiryDays": 7
}
```

**✅ JWT Token Structure:**
```csharp
// Claims to include:
- sub (userId)
- email
- username
- jti (unique token ID)
- iat (issued at)
- exp (expiry)
```

---

## Step 4: Security Requirements

**✅ Password Verification:**
```csharp
var isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

**✅ Account Lockout Logic:**
```csharp
// Check if locked
if (user.IsLocked && user.LockedUntil > DateTime.UtcNow)
    throw new InvalidOperationException(ErrorMessages.Authentication.AccountLocked);

// Increment failed attempts
user.FailedLoginAttempts++;
user.LastFailedLoginAttempt = DateTime.UtcNow;

if (user.FailedLoginAttempts >= 5)
{
    user.IsLocked = true;
    user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
}

// Reset on success
user.FailedLoginAttempts = 0;
user.LastLoginAt = DateTime.UtcNow;
user.LastLoginIp = ipAddress;
```

**✅ Refresh Token Storage:**
```csharp
// Store in UserSession table
var session = new UserSession
{
    UserId = user.Id,
    RefreshToken = GenerateCryptoRandomString(32),
    RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
    IpAddress = ipAddress,
    UserAgent = userAgent,
    IsActive = true
};
```

---

## Step 5: Testing Requirements

### Unit Tests (Application.Tests):

**AuthServiceTests.cs:**
```
✅ LoginAsync_ValidCredentials_ReturnsLoginResponse
✅ LoginAsync_ValidCredentials_GeneratesAccessToken
✅ LoginAsync_ValidCredentials_GeneratesRefreshToken
✅ LoginAsync_ValidCredentials_UpdatesLastLoginInfo
✅ LoginAsync_ValidCredentials_ResetsFailedAttempts
✅ LoginAsync_InvalidPassword_ThrowsInvalidOperationException
✅ LoginAsync_InvalidPassword_IncrementsFailedAttempts
✅ LoginAsync_FifthFailedAttempt_LocksAccount
✅ LoginAsync_LockedAccount_ThrowsInvalidOperationException
✅ LoginAsync_InactiveAccount_ThrowsInvalidOperationException
✅ LoginAsync_UnverifiedEmail_ThrowsInvalidOperationException
✅ LoginAsync_NonExistentEmail_ThrowsInvalidOperationException
✅ RefreshTokenAsync_ValidToken_ReturnsNewAccessToken
✅ RefreshTokenAsync_ExpiredToken_ThrowsInvalidOperationException
✅ RefreshTokenAsync_InvalidToken_ThrowsInvalidOperationException
```

**LoginRequestValidatorTests.cs:**
```
✅ Validate_ValidData_PassesValidation
✅ Validate_EmptyEmail_FailsValidation
✅ Validate_InvalidEmailFormat_FailsValidation
✅ Validate_EmptyPassword_FailsValidation
```

### Integration Tests (API.Tests):

**AuthControllerTests.cs (add to existing file):**
```
✅ Login_ValidCredentials_Returns200OK
✅ Login_ValidCredentials_ReturnsAccessAndRefreshToken
✅ Login_ValidCredentials_UpdatesLastLoginInDatabase
✅ Login_InvalidPassword_Returns400BadRequest
✅ Login_FiveFailedAttempts_LocksAccount
✅ Login_LockedAccount_Returns400BadRequest
✅ Login_NonExistentEmail_Returns400BadRequest
✅ Login_MissingFields_Returns400BadRequest
✅ RefreshToken_ValidToken_Returns200OK
✅ RefreshToken_ExpiredToken_Returns400BadRequest
```

---

## Step 6: Documentation Updates

**Update these README files:**

**✅ PutZige.Application/README.md:**
- Add AuthService to services list
- Add JWT token generation info
- Add refresh token pattern

**✅ PutZige.Infrastructure/README.md:**
- Add JwtTokenService info
- Add JwtSettings configuration

**✅ PutZige.API/README.md:**
- Add POST /api/v1/auth/login endpoint
- Add POST /api/v1/auth/refresh-token endpoint
- Add JWT authentication setup

---

## Step 7: Build & Test Process

### Execute in this order:

**1. Build Solution:**
```bash
dotnet build
# Fix any compilation errors before proceeding
```

**2. Run Unit Tests:**
```bash
dotnet test PutZige.Application.Tests --verbosity normal
# All tests must pass (including new AuthService tests)
```

**3. Run Integration Tests:**
```bash
dotnet test PutZige.API.Tests --verbosity normal
# All tests must pass (including new login endpoint tests)
```

**4. Run All Tests:**
```bash
dotnet test --verbosity normal
# Final verification - everything passes
```

**5. Manual Testing:**
```bash
# Start API
dotnet run --project PutZige.API

# Test login with Postman/curl
POST http://localhost:5000/api/v1/auth/login
Body: { "email": "user@test.com", "password": "ValidPass123!" }

# Expected response:
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "abc123...",
    "expiresIn": 900,
    "user": { ... }
  },
  "message": "Login successful"
}
```

---

## Step 8: Report Format

After completing each phase, report:

```markdown
## Phase X Complete

### Changes Made:
1. ✅ Created LoginRequest.cs, LoginResponse.cs
2. ✅ Implemented AuthService with login logic
3. ✅ Added JWT token generation
4. ✅ [list all files created/modified]

### Build Status:
✅ Solution builds successfully
❌ Build failed: [error details]

### Test Status:
Unit Tests: ✅ 45 passed, 0 failed
Integration Tests: ✅ 12 passed, 0 failed
Total: ✅ 57 passed, 0 failed

### Issues Found:
1. [Issue description] - Fixed by [solution]
2. [Issue description] - Fixed by [solution]

### Next Phase:
[What's next]
```

---

## Quality Checklist

Before marking complete, verify:

- [ ] Follows existing code patterns (validation, services, controllers)
- [ ] Uses FluentValidation with .WithName("lowercase")
- [ ] Returns ApiResponse<T> wrapper
- [ ] Includes concise, one-liner logging
- [ ] Error messages in ErrorMessages.cs
- [ ] JWT tokens properly signed and validated
- [ ] Refresh tokens stored in UserSession table
- [ ] Account lockout logic works (5 attempts, 15 min lock)
- [ ] Password verified with BCrypt
- [ ] All tests pass (unit + integration)
- [ ] README files updated
- [ ] Solution builds without errors
- [ ] Follows clean architecture (no layer violations)
- [ ] All async methods use CancellationToken
- [ ] Proper null checks and exception handling

---

## Start Now

**Begin with Phase 1:**
1. Analyze existing patterns (list what you found)
2. Create DTOs (LoginRequest, LoginResponse, etc.)
3. Create validators
4. Implement AuthService
5. Add JWT token service
6. Update AuthController
7. Write tests
8. Build & verify
9. Update documentation

**Report after each phase - don't skip ahead until current phase is complete and tested.**