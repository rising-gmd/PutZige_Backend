// PutZige.Application.Tests/Builders/UserBuilder.cs
#nullable enable
using System;
using PutZige.Domain.Entities;

namespace PutZige.Application.Tests.Builders
{
    /// <summary>
    /// Fluent test builder for creating User entities with sensible defaults.
    /// </summary>
    public class UserBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _email = "test@test.com";
        private string _username = "testuser";
        private string _displayName = "Test User";
        private string _passwordHash = "hashed";

        public UserBuilder WithEmail(string email)
        {
            _email = email; return this;
        }

        public UserBuilder WithUsername(string username)
        {
            _username = username; return this;
        }

        public UserBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName; return this;
        }

        public UserBuilder WithPasswordHash(string hash)
        {
            _passwordHash = hash; return this;
        }

        public User Build() => new User
        {
            Id = _id,
            Email = _email,
            Username = _username,
            DisplayName = _displayName,
            PasswordHash = _passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }
}
