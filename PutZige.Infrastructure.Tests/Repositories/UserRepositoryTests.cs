// PutZige.Infrastructure.Tests/Repositories/UserRepositoryTests.cs
#nullable enable
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using PutZige.Infrastructure.Tests.Repositories;
using PutZige.Infrastructure.Data;
using PutZige.Infrastructure.Repositories;
using PutZige.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PutZige.Infrastructure.Tests.Repositories
{
    public class UserRepositoryTests : DatabaseTestBase
    {
        private readonly UserRepository _sut;

        public UserRepositoryTests() : base()
        {
            _sut = new UserRepository(Context);
        }

        /// <summary>
        /// Returns true when an email exists in the database.
        /// </summary>
        [Fact]
        public async Task IsEmailTakenAsync_ExistingEmail_ReturnsTrue()
        {
            // Arrange
            var email = "existing@test.com";
            var user = new User { Email = email, Username = "u1", PasswordHash = "h", DisplayName = "d" };
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            // Act
            var result = await _sut.IsEmailTakenAsync(email);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Returns false when email does not exist.
        /// </summary>
        [Fact]
        public async Task IsEmailTakenAsync_NonExistingEmail_ReturnsFalse()
        {
            // Arrange
            var email = "notfound@test.com";

            // Act
            var result = await _sut.IsEmailTakenAsync(email);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Returns true when username exists in the database.
        /// </summary>
        [Fact]
        public async Task IsUsernameTakenAsync_ExistingUsername_ReturnsTrue()
        {
            // Arrange
            var username = "existinguser";
            var user = new User { Email = "e@t.com", Username = username, PasswordHash = "h", DisplayName = "d" };
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            // Act
            var result = await _sut.IsUsernameTakenAsync(username);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Returns false when username does not exist.
        /// </summary>
        [Fact]
        public async Task IsUsernameTakenAsync_NonExistingUsername_ReturnsFalse()
        {
            // Arrange
            var username = "nouser";

            // Act
            var result = await _sut.IsUsernameTakenAsync(username);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Retrieves a user by email when present.
        /// </summary>
        [Fact]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            // Arrange
            var email = "findme@test.com";
            var user = new User { Email = email, Username = "u2", PasswordHash = "h", DisplayName = "d" };
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        /// <summary>
        /// Returns null when email not found.
        /// </summary>
        [Fact]
        public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Arrange
            var email = "missing@test.com";

            // Act
            var result = await _sut.GetByEmailAsync(email);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Adds a user and persists it to the database.
        /// </summary>
        [Fact]
        public async Task AddAsync_ValidUser_SavesSuccessfully()
        {
            // Arrange
            var email = "new@test.com";
            var user = new User { Email = email, Username = "newuser", PasswordHash = "h", DisplayName = "d" };

            // Act
            await _sut.AddAsync(user);
            await Context.SaveChangesAsync();

            // Assert
            var stored = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            stored.Should().NotBeNull();
            stored!.Username.Should().Be("newuser");
        }

        /// <summary>
        /// Soft delete sets flags and deletion timestamp.
        /// </summary>
        [Fact]
        public async Task Delete_ExistingUser_SetsSoftDeleteFlags()
        {
            // Arrange
            var email = "todelete@test.com";
            var user = new User { Email = email, Username = "deluser", PasswordHash = "h", DisplayName = "d" };
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            // Act
            _sut.Delete(user);
            await Context.SaveChangesAsync();

            // Assert
            var stored = await Context.Users.AsNoTracking().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);
            stored.Should().NotBeNull();
            stored!.IsDeleted.Should().BeTrue();
            stored.DeletedAt.Should().NotBeNull();
        }
    }
}
