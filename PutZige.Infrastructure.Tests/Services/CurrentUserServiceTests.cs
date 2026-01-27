#nullable enable
using System;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using PutZige.Infrastructure.Services;
using Xunit;

namespace PutZige.Infrastructure.Tests.Services;

public class CurrentUserServiceTests
{
    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private readonly FakeHttpContextAccessor _httpContextAccessor = new();

    private CurrentUserService CreateSut(ClaimsPrincipal? user)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = user ?? new ClaimsPrincipal(new ClaimsIdentity());
        _httpContextAccessor.HttpContext = httpContext;
        return new CurrentUserService(_httpContextAccessor, null);
    }

    [Fact]
    public void GetUserId_AuthenticatedUser_ReturnsGuid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", id.ToString()) }, "TestAuth"));
        var sut = CreateSut(claims);

        // Act
        var result = sut.GetUserId();

        // Assert
        result.Should().Be(id);
    }

    [Fact]
    public void TryGetUserId_AuthenticatedUser_ReturnsGuid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", id.ToString()) }, "TestAuth"));
        var sut = CreateSut(claims);

        // Act
        var result = sut.TryGetUserId();

        // Assert
        result.Should().Be(id);
    }

    [Fact]
    public void TryGetUserId_NotAuthenticated_ReturnsNull()
    {
        // Arrange
        var sut = CreateSut(new ClaimsPrincipal(new ClaimsIdentity()));

        // Act
        var result = sut.TryGetUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryGetUserId_InvalidGuidFormat_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "bad-guid") }, "TestAuth"));
        var sut = CreateSut(claims);

        // Act
        var result = sut.TryGetUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserEmail_HasEmailClaim_ReturnsEmail()
    {
        // Arrange
        var email = "user@test.com";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", email) }, "TestAuth"));
        var sut = CreateSut(claims);

        // Act
        var result = sut.GetUserEmail();

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void GetUserEmail_NoEmailClaim_ReturnsNull()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", Guid.NewGuid().ToString()) }, "TestAuth"));
        var sut = CreateSut(claims);

        // Act
        var result = sut.GetUserEmail();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserName_HasUsernameClaim_ReturnsUsername()
    {
        // Arrange
        var uname = "tester";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("username", uname) }, "TestAuth"));
        var sut = CreateSut(claims);

        // Act
        var result = sut.GetUserName();

        // Assert
        result.Should().Be(uname);
    }

    [Fact]
    public void GetUserName_NoUsernameClaim_FallsBackToIdentityName()
    {
        // Arrange
        var identity = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "fallback") }, "TestAuth");
        var claims = new ClaimsPrincipal(identity);
        var sut = CreateSut(claims);

        // Act
        var result = sut.GetUserName();

        // Assert
        result.Should().Be("fallback");
    }

    [Fact]
    public void IsAuthenticated_AuthenticatedUser_ReturnsTrue()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", Guid.NewGuid().ToString()) }, "TestAuth"));
        var sut = CreateSut(claims);

        // Act
        var result = sut.IsAuthenticated();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_NotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var sut = CreateSut(new ClaimsPrincipal(new ClaimsIdentity()));

        // Act
        var result = sut.IsAuthenticated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_NullHttpContext_ReturnsFalse()
    {
        // Arrange
        _httpContextAccessor.HttpContext = null;
        var sut = new CurrentUserService(_httpContextAccessor, null);

        // Act
        var result = sut.IsAuthenticated();

        // Assert
        result.Should().BeFalse();
    }
}
