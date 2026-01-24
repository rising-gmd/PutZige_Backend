// PutZige.API.Tests/Controllers/AuthControllerTests.cs
#nullable enable
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using PutZige.API.Tests.Integration;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.DTOs.Common;
using PutZige.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PutZige.API.Tests.Controllers
{
    public class AuthControllerTests : IntegrationTestBase
    {
        private static (string hash, string salt) CreateHash(string plain)
        {
            var salt = new byte[32];
            RandomNumberGenerator.Fill(salt);
            var derived = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(plain), salt, 100000, HashAlgorithmName.SHA512, 64);
            return (Convert.ToBase64String(derived), Convert.ToBase64String(salt));
        }

        private static bool VerifyHash(string plain, string hashBase64, string saltBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);
            var derived = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(plain), salt, 100000, HashAlgorithmName.SHA512, 64);
            var derivedBase64 = Convert.ToBase64String(derived);
            var a = Convert.FromBase64String(derivedBase64);
            var b = Convert.FromBase64String(hashBase64);
            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        /// <summary>
        /// Registers a user via API and returns HTTP 201 Created.
        /// </summary>
        [Fact]
        public async Task Register_ValidRequest_Returns201Created()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "inttest@example.com",
                Username = "intuser",
                DisplayName = "Integration User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        /// <summary>
        /// Successful registration returns a user DTO in response.
        /// </summary>
        [Fact]
        public async Task Register_ValidRequest_ReturnsUserResponseDto()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "inttest2@example.com",
                Username = "intuser2",
                DisplayName = "Integration User 2",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterUserResponse>>();
            payload.Should().NotBeNull();
            payload!.Data.Should().NotBeNull();
            payload.Data!.Email.Should().Be(request.Email);
            payload.Data.Username.Should().Be(request.Username);
        }

        /// <summary>
        /// Registers a user and verifies the user exists in the database.
        /// </summary>
        [Fact]
        public async Task Register_ValidRequest_SavesUserToDatabase()
        {
            // Arrange
            var email = "saveduser@example.com";
            var request = new RegisterUserRequest
            {
                Email = email,
                Username = "saveduser",
                DisplayName = "Saved User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Assert: query DB to ensure user exists
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();
            user!.Email.Should().Be(email);
        }

        /// <summary>
        /// Registration endpoint stores hashed password and salt in DB.
        /// </summary>
        [Fact]
        public async Task Register_ValidRequest_HashesPassword()
        {
            // Arrange
            var email = "hashuser@example.com";
            var plain = "Password123!";
            var request = new RegisterUserRequest
            {
                Email = email,
                Username = "hashuser",
                DisplayName = "Hash User",
                Password = plain,
                ConfirmPassword = plain
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Assert
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();
            user!.PasswordHash.Should().NotBeNullOrWhiteSpace();
            user.PasswordSalt.Should().NotBeNullOrWhiteSpace();
            VerifyHash(plain, user.PasswordHash, user.PasswordSalt).Should().BeTrue();
        }

        /// <summary>
        /// Duplicate email registration returns BadRequest.
        /// </summary>
        [Fact]
        public async Task Register_DuplicateEmail_Returns400BadRequest()
        {
            // Arrange
            var email = "dupe@example.com";

            // Seed existing user
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash("P@ss1");
                await db.Users.AddAsync(new User { Email = email, Username = "u", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, DisplayName = "d" });
                await db.SaveChangesAsync();
            }

            var request = new RegisterUserRequest
            {
                Email = email,
                Username = "newuser",
                DisplayName = "New User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Missing required fields return BadRequest with lowercase field names in errors.
        /// </summary>
        [Fact]
        public async Task Register_MissingRequiredFields_Returns400WithLowercaseFieldNames()
        {
            // Arrange
            var invalidRequest = new { username = "test" }; // Missing email, password

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", invalidRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().ContainKey("email");
            result.Errors.Should().ContainKey("password");
        }

        /// <summary>
        /// Invalid email format returns BadRequest referencing the email field.
        /// </summary>
        [Fact]
        public async Task Register_InvalidEmailFormat_Returns400WithLowercaseFieldName()
        {
            // Arrange
            var invalidRequest = new
            {
                email = "notanemail",
                username = "testuser",
                displayName = "Test User",
                password = "ValidPass123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", invalidRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Should().NotBeNull();
            result!.Errors.Should().ContainKey("email");
        }

        /// <summary>
        /// Successful login returns tokens and HTTP 200.
        /// </summary>
        [Fact]
        public async Task Login_ValidCredentials_Returns200OK()
        {
            // Arrange: seed user
            var email = "intlogin@example.com";
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash("Password123!");
                db.Users.Add(new User { Email = email, Username = "intlogin", DisplayName = "Int Login", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, IsEmailVerified = true, IsActive = true });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequest { Email = email, Password = "Password123!" };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
            payload.Should().NotBeNull();
            payload!.Data.Should().NotBeNull();
            payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Invalid login password returns BadRequest.
        /// </summary>
        [Fact]
        public async Task Login_InvalidPassword_Returns400BadRequest()
        {
            var email = "intbadpass@example.com";
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash("RightPass1!");
                db.Users.Add(new User { Email = email, Username = "badpass", DisplayName = "Bad Pass", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, IsEmailVerified = true, IsActive = true });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequest { Email = email, Password = "WrongPass!" };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Five failed login attempts from API lock the user in DB.
        /// </summary>
        [Fact]
        public async Task Login_FiveFailedAttempts_LocksAccount()
        {
            // Arrange
            var email = "lockme@example.com";
            var correct = "Correct1!";
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(correct);
                db.Users.Add(new User { Email = email, Username = "lockme", DisplayName = "Lock Me", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, IsEmailVerified = true, IsActive = true });
                await db.SaveChangesAsync();
            }

            var wrongRequest = new LoginRequest { Email = email, Password = "Wrong1!" };

            // Act: perform 5 failed attempts
            for (int i = 0; i < 5; i++)
            {
                var r = await Client.PostAsJsonAsync("/api/v1/auth/login", wrongRequest);
                r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            // Assert: user is locked in DB
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
                user.Should().NotBeNull();
                user!.IsLocked.Should().BeTrue();
                user.LockedUntil.Should().BeAfter(DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Login to a locked account returns BadRequest.
        /// </summary>
        [Fact]
        public async Task Login_LockedAccount_Returns400BadRequest()
        {
            var email = "prelocked@example.com";
            var password = "P@ssword1";
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(password);
                db.Users.Add(new User { Email = email, Username = "prelocked", DisplayName = "Pre Locked", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, IsEmailVerified = true, IsActive = true, IsLocked = true, LockedUntil = DateTime.UtcNow.AddMinutes(15) });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequest { Email = email, Password = password };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Non-existent email returns BadRequest.
        /// </summary>
        [Fact]
        public async Task Login_NonExistentEmail_Returns400BadRequest()
        {
            var request = new LoginRequest { Email = "doesnotexist@example.com", Password = "Whatever1!" };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Missing login fields returns BadRequest and mentions email in errors.
        /// </summary>
        [Fact]
        public async Task Login_MissingFields_Returns400BadRequest()
        {
            var invalidRequest = new { password = "Password1!" }; // missing email
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", invalidRequest);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            result.Should().NotBeNull();
            result!.Errors.Should().NotBeNull();
            // Accept either explicit 'email' field or JSON-deserialization root errors that mention email
            var hasEmailKey = result.Errors.ContainsKey("email");
            var hasEmailInMessages = result.Errors.Values.SelectMany(v => v ?? Array.Empty<string>()).Any(m => m != null && m.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0);
            (hasEmailKey || hasEmailInMessages).Should().BeTrue("expected validation to mention the missing 'email' field in either key or message");
        }

        /// <summary>
        /// Valid refresh token returns new tokens and HTTP 200.
        /// </summary>
        [Fact]
        public async Task RefreshToken_ValidToken_Returns200OK()
        {
            // Arrange: seed user with session
            var email = "intrefresh@example.com";
            string refresh = "seed-refresh-token";
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(refresh);
                var user = new User { Email = email, Username = "refuser", DisplayName = "Ref User", PasswordHash = CreateHash("Pass1!").hash, PasswordSalt = CreateHash("Pass1!").salt, IsEmailVerified = true, IsActive = true };
                user.Session = new UserSession { RefreshTokenHash = hashed.hash, RefreshTokenSalt = hashed.salt, RefreshTokenExpiry = DateTime.UtcNow.AddDays(1), IsOnline = true };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            var request = new RefreshTokenRequest { RefreshToken = refresh };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>();
            payload.Should().NotBeNull();
            payload!.Data.Should().NotBeNull();
            payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Expired refresh token returns BadRequest.
        /// </summary>
        [Fact]
        public async Task RefreshToken_ExpiredToken_Returns400BadRequest()
        {
            // Arrange: seed user with expired token
            var email = "expiredrefresh@example.com";
            string refresh = "expired-token";
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(refresh);
                var user = new User { Email = email, Username = "expiredref", DisplayName = "Expired Ref", PasswordHash = CreateHash("Pass1!").hash, PasswordSalt = CreateHash("Pass1!").salt, IsEmailVerified = true, IsActive = true };
                user.Session = new UserSession { RefreshTokenHash = hashed.hash, RefreshTokenSalt = hashed.salt, RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1), IsOnline = true };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            var request = new RefreshTokenRequest { RefreshToken = refresh };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
