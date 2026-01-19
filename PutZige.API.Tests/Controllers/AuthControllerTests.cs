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

namespace PutZige.API.Tests.Controllers
{
    public class AuthControllerTests : IntegrationTestBase
    {
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
            BCrypt.Net.BCrypt.Verify(plain, user.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400BadRequest()
        {
            // Arrange
            var email = "dupe@example.com";

            // Seed existing user
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Users.AddAsync(new User { Email = email, Username = "u", PasswordHash = "h", DisplayName = "d" });
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

        [Fact]
        public async Task Register_MissingRequiredFields_Returns400BadRequest()
        {
            // Arrange: missing password
            var request = new { Email = "a@b.com", Username = "u", DisplayName = "d" };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_InvalidEmailFormat_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "not-an-email",
                Username = "uinvalid",
                DisplayName = "Invalid Email",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
