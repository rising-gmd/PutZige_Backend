// PutZige.API.Tests/Controllers/AuthControllerTests.cs
#nullable enable
using System;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PutZige.API.Tests.Integration;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.DTOs.Common;
using PutZige.Domain.Entities;
using PutZige.Infrastructure.Data;
using Xunit;

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

        [Fact]
        public async Task Login_ValidCredentials_Returns200AndLoginResponse()
        {
            var email = "intauth@example.com";
            var password = "Password123!";

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(password);
                await db.Users.AddAsync(new User
                {
                    Email = email,
                    Username = "intauth",
                    DisplayName = "Auth User",
                    PasswordHash = hashed.hash,
                    PasswordSalt = hashed.salt,
                    IsActive = true,
                    IsEmailVerified = true
                });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequest { Email = email, Password = password };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
            payload.Should().NotBeNull();
            payload!.IsSuccess.Should().BeTrue();
            payload.Data.Should().NotBeNull();
            payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.Email.Should().Be(email);
        }

        [Fact]
        public async Task Login_InvalidPassword_Returns400InvalidCredentials()
        {
            var email = "badpass@example.com";
            var password = "Correct1!";

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(password);
                await db.Users.AddAsync(new User { Email = email, Username = "bp", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, IsActive = true, IsEmailVerified = true, DisplayName = "B" });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequest { Email = email, Password = "WrongPass!" };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            payload.Should().NotBeNull();
            payload!.IsSuccess.Should().BeFalse();
            payload.Message.Should().Contain("Invalid");
        }

        [Fact]
        public async Task Login_MissingRequiredFields_Returns400WithLowercaseFieldNames()
        {
            var invalid = new { username = "noemail" };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", invalid);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            result.Should().NotBeNull();
            result!.Errors.Should().NotBeNull();
            result.Errors!.Should().ContainKey("email");
            result.Errors.Should().ContainKey("password");
        }

        [Fact]
        public async Task Login_UnverifiedEmail_Returns400()
        {
            var email = "unverified@example.com";
            var password = "Password1!";

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(password);
                await db.Users.AddAsync(new User { Email = email, Username = "uv", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, IsActive = true, IsEmailVerified = false, DisplayName = "U" });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequest { Email = email, Password = password };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            payload.Should().NotBeNull();
            payload!.Message.ToLowerInvariant().Should().Contain("verify");
        }

        [Fact]
        public async Task Login_InactiveAccount_Returns400()
        {
            var email = "inactive@example.com";
            var password = "Password1!";

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashed = CreateHash(password);
                await db.Users.AddAsync(new User { Email = email, Username = "inact", PasswordHash = hashed.hash, PasswordSalt = hashed.salt, IsActive = false, IsEmailVerified = true, DisplayName = "I" });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequest { Email = email, Password = password };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            payload.Should().NotBeNull();
            payload!.Message.ToLowerInvariant().Should().Contain("inactive");
        }

        [Fact]
        public async Task RefreshToken_ValidRequest_Returns200AndRotatesToken()
        {
            var email = "rtuser@example.com";
            var refresh = "refresh-token-plain-123";

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var pwd = CreateHash("Password1!");
                var rt = CreateHash(refresh);

                var user = new User
                {
                    Email = email,
                    Username = "rtu",
                    DisplayName = "RT User",
                    PasswordHash = pwd.hash,
                    PasswordSalt = pwd.salt,
                    IsActive = true,
                    IsEmailVerified = true,
                    Session = new UserSession
                    {
                        RefreshTokenHash = rt.hash,
                        RefreshTokenSalt = rt.salt,
                        RefreshTokenExpiry = DateTime.UtcNow.AddDays(1),
                        IsOnline = true
                    }
                };

                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
            }

            var request = new RefreshTokenRequest { RefreshToken = refresh };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>();
            payload.Should().NotBeNull();
            payload!.IsSuccess.Should().BeTrue();
            payload.Data.Should().NotBeNull();
            payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = await db.Users.Include(u => u.Session).AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
                user.Should().NotBeNull();
                user!.Session.Should().NotBeNull();
                // After refresh, stored hash should not equal the original raw token
                VerifyHash(refresh, user.Session!.RefreshTokenHash ?? string.Empty, user.Session.RefreshTokenSalt ?? string.Empty).Should().BeFalse();
            }
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_Returns400()
        {
            var request = new RefreshTokenRequest { RefreshToken = "this-does-not-exist" };
            var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            payload.Should().NotBeNull();
            payload!.IsSuccess.Should().BeFalse();
            payload.Message.ToLowerInvariant().Should().Contain("refresh");
        }
    }
}
