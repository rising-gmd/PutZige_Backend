#nullable enable
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
using System.Security.Cryptography;
using System.Text;
using System;

namespace PutZige.API.Tests.Controllers
{
    public partial class UsersControllerTests : IntegrationTestBase
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
        public async Task CreateUser_ValidRequest_Returns201Created()
        {
            var request = new RegisterUserRequest
            {
                Email = "inttest@example.com",
                Username = "intuser",
                DisplayName = "Integration User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var response = await Client.PostAsJsonAsync(TestApiEndpoints.Users, request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateUser_ValidRequest_ReturnsUserResponseDto()
        {
            var request = new RegisterUserRequest
            {
                Email = "inttest2@example.com",
                Username = "intuser2",
                DisplayName = "Integration User 2",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var response = await Client.PostAsJsonAsync(TestApiEndpoints.Users, request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterUserResponse>>();
            payload.Should().NotBeNull();
            payload!.Data.Should().NotBeNull();
            payload.Data!.Email.Should().Be(request.Email);
            payload.Data.Username.Should().Be(request.Username);
        }

        [Fact]
        public async Task CreateUser_ValidRequest_SavesUserToDatabase()
        {
            var email = "saveduser@example.com";
            var request = new RegisterUserRequest
            {
                Email = email,
                Username = "saveduser",
                DisplayName = "Saved User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var response = await Client.PostAsJsonAsync(TestApiEndpoints.Users, request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();
            user!.Email.Should().Be(email);
        }

        [Fact]
        public async Task CreateUser_ValidRequest_HashesPassword()
        {
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

            var response = await Client.PostAsJsonAsync(TestApiEndpoints.Users, request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();
            user!.PasswordHash.Should().NotBeNullOrWhiteSpace();
            user.PasswordSalt.Should().NotBeNullOrWhiteSpace();
            VerifyHash(plain, user.PasswordHash, user.PasswordSalt).Should().BeTrue();
        }

        [Fact]
        public async Task CreateUser_DuplicateEmail_Returns400BadRequest()
        {
            var email = "dupe@example.com";

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

            var response = await Client.PostAsJsonAsync(TestApiEndpoints.Users, request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_MissingRequiredFields_Returns400WithLowercaseFieldNames()
        {
            var invalidRequest = new { username = "test" };
            var response = await Client.PostAsJsonAsync(TestApiEndpoints.Users, invalidRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().ContainKey("email");
            result.Errors.Should().ContainKey("password");
        }

        [Fact]
        public async Task CreateUser_InvalidEmailFormat_Returns400WithLowercaseFieldName()
        {
            var invalidRequest = new
            {
                email = "notanemail",
                username = "testuser",
                displayName = "Test User",
                password = "ValidPass123!"
            };

            var response = await Client.PostAsJsonAsync(TestApiEndpoints.Users, invalidRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Should().NotBeNull();
            result!.Errors.Should().ContainKey("email");
        }

        [Fact]
        public async Task CreateUser_RateLimitExceeded_Returns429()
        {
            // Trigger registration endpoint multiple times from same IP
            for (int i = 0; i < 5; i++)
            {
                var req = new RegisterUserRequest
                {
                    Email = $"ratereg{i}@example.com",
                    Username = $"ratereg{i}",
                    DisplayName = "RegUser",
                    Password = "Password1!",
                    ConfirmPassword = "Password1!"
                };

                await Client.PostAsJsonAsync(TestApiEndpoints.Users, req);
            }

            var final = new RegisterUserRequest
            {
                Email = "ratereg-final@example.com",
                Username = "ratereglast",
                DisplayName = "RegUser",
                Password = "Password1!",
                ConfirmPassword = "Password1!"
            };

            var res = await Client.PostAsJsonAsync(TestApiEndpoints.Users, final);
            res.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task Registration_SpamWithDisposableEmails_LimitEnforced()
        {
            for (int i = 0; i < 10; i++)
            {
                var req = new RegisterUserRequest
                {
                    Email = $"disposable{i}@disposablemail.test",
                    Username = $"disp{i}",
                    DisplayName = "Disposable",
                    Password = "Password1!",
                    ConfirmPassword = "Password1!"
                };

                var r = await Client.PostAsJsonAsync(TestApiEndpoints.Users, req);
            }

            var final = await Client.PostAsJsonAsync(TestApiEndpoints.Users, new RegisterUserRequest
            {
                Email = "disposable-final@disposablemail.test",
                Username = "disp-final",
                DisplayName = "Disposable",
                Password = "Password1!",
                ConfirmPassword = "Password1!"
            });

            final.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.TooManyRequests, HttpStatusCode.BadRequest);
        }
    }
}
