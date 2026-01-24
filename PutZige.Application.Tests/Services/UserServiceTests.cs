#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Bogus;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PutZige.Application.Services;
using PutZige.Application.Interfaces;
using PutZige.Application.DTOs;
using PutZige.Application.DTOs.Auth;
using PutZige.Domain.Interfaces;
using PutZige.Domain.Entities;
using PutZige.Application.Common.Messages;
using PutZige.Application.Common.Constants;

namespace PutZige.Application.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly Mock<IHashingService> _mockHashingService;
        private readonly UserService _sut;
        private readonly Faker _faker;
        private readonly CancellationToken _ct = CancellationToken.None;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockHashingService = new Mock<IHashingService>();
            _faker = new Faker();

            _mockHashingService.Setup(h => h.GenerateSecureToken(It.IsAny<int>())).Returns(() => Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+","-").Replace("/","_").TrimEnd('='));
            _mockHashingService.Setup(h => h.HashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((string s, CancellationToken ct) => new HashedValue("hash-"+s, "salt-"+s));

            _sut = new UserService(
                _mockUserRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockHashingService.Object,
                _mockLogger.Object);
        }

        public void Dispose()
        {
            // No unmanaged resources to dispose; mocks will be GC'ed
        }

        [Fact]
        public async Task RegisterUserAsync_ValidData_ReturnsUserResponse()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            _mockUserRepository
                .Setup(x => x.IsEmailTakenAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockUserRepository
                .Setup(x => x.IsUsernameTakenAsync(username, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Capture the user passed to AddAsync for later assertions
            User? capturedUser = null;
            _mockUserRepository
                .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<RegisterUserResponse>(It.IsAny<User>()))
                .Returns((User u) => new RegisterUserResponse
                {
                    UserId = u.Id,
                    Email = u.Email ?? string.Empty,
                    Username = u.Username ?? string.Empty,
                    DisplayName = u.DisplayName ?? string.Empty,
                    IsEmailVerified = u.IsEmailVerified,
                    CreatedAt = u.CreatedAt
                });

            // Act
            var result = await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            result.Username.Should().Be(username);
            result.DisplayName.Should().Be(displayName);

            _mockUserRepository.Verify(x => x.IsEmailTakenAsync(email, It.IsAny<CancellationToken>()), Times.Once);
            _mockUserRepository.Verify(x => x.IsUsernameTakenAsync(username, It.IsAny<CancellationToken>()), Times.Once);
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            capturedUser.Should().NotBeNull();
            capturedUser!.Email.Should().Be(email);
            capturedUser.Username.Should().Be(username);
            capturedUser.DisplayName.Should().Be(displayName);
            capturedUser.IsEmailVerified.Should().BeFalse();
            capturedUser.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();
            capturedUser.EmailVerificationTokenExpiry.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidData_HashesPassword()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            _mockUserRepository.Setup(x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.IsUsernameTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            User? capturedUser = null;
            _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<RegisterUserResponse>(It.IsAny<User>())).Returns((User u) => new RegisterUserResponse
            {
                UserId = u.Id,
                Email = u.Email ?? string.Empty,
                Username = u.Username ?? string.Empty,
                DisplayName = u.DisplayName ?? string.Empty,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt
            });

            // Act
            var result = await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser!.PasswordHash.Should().NotBeNullOrWhiteSpace();
            capturedUser.PasswordHash.Should().Be("hash-" + password);
            capturedUser.PasswordSalt.Should().Be("salt-" + password);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidData_GeneratesVerificationToken()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            _mockUserRepository.Setup(x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.IsUsernameTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            User? capturedUser = null;
            _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<RegisterUserResponse>(It.IsAny<User>())).Returns((User u) => new RegisterUserResponse
            {
                UserId = u.Id,
                Email = u.Email ?? string.Empty,
                Username = u.Username ?? string.Empty,
                DisplayName = u.DisplayName ?? string.Empty,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt
            });

            // Act
            var result = await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser!.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();
            // Token should be URL-safe base64 (no '+' or '/') and not end with '=' padding
            capturedUser.EmailVerificationToken.Should().NotContain("+");
            capturedUser.EmailVerificationToken.Should().NotContain("/");
            capturedUser.EmailVerificationToken.Should().NotEndWith("=");

            // Expiry approximately 7 days from now (allow small tolerance)
            var expectedExpiry = DateTime.UtcNow.AddDays(AppConstants.Security.EmailVerificationTokenExpirationDays);
            capturedUser.EmailVerificationTokenExpiry.Should().BeCloseTo(expectedExpiry, precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task RegisterUserAsync_ValidData_SavesUserToRepository()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            _mockUserRepository.Setup(x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.IsUsernameTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<RegisterUserResponse>(It.IsAny<User>())).Returns((User u) => new RegisterUserResponse
            {
                UserId = u.Id,
                Email = u.Email ?? string.Empty,
                Username = u.Username ?? string.Empty,
                DisplayName = u.DisplayName ?? string.Empty,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt
            });

            // Act
            var result = await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            _mockUserRepository.Verify(x => x.AddAsync(It.Is<User>(u => u.Email == email && u.Username == username), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidData_MapsToResponseDto()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            _mockUserRepository.Setup(x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.IsUsernameTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var responseReturned = new RegisterUserResponse
            {
                UserId = Guid.NewGuid(),
                Email = email,
                Username = username,
                DisplayName = displayName,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockMapper.Setup(m => m.Map<RegisterUserResponse>(It.IsAny<User>())).Returns(responseReturned);

            // Act
            var result = await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            _mockMapper.Verify(m => m.Map<RegisterUserResponse>(It.IsAny<User>()), Times.Once);
            result.Should().NotBeNull();
            result.Email.Should().Be(responseReturned.Email);
            result.Username.Should().Be(responseReturned.Username);
            result.DisplayName.Should().Be(responseReturned.DisplayName);
            result.UserId.Should().Be(responseReturned.UserId);
        }

        [Fact]
        public async Task RegisterUserAsync_DuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            _mockUserRepository.Setup(x => x.IsEmailTakenAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ErrorMessages.Authentication.EmailAlreadyTaken);

            _mockUserRepository.Verify(x => x.IsEmailTakenAsync(email, It.IsAny<CancellationToken>()), Times.Once);
            _mockUserRepository.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_DuplicateUsername_ThrowsInvalidOperationException()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            _mockUserRepository.Setup(x => x.IsEmailTakenAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.IsUsernameTakenAsync(username, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ErrorMessages.Authentication.UsernameAlreadyTaken);

            _mockUserRepository.Verify(x => x.IsEmailTakenAsync(email, It.IsAny<CancellationToken>()), Times.Once);
            _mockUserRepository.Verify(x => x.IsUsernameTakenAsync(username, It.IsAny<CancellationToken>()), Times.Once);
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_NullEmail_ThrowsArgumentException()
        {
            // Arrange
            string? email = null;
            var username = _faker.Internet.UserName();
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(email!, username, displayName, password, _ct);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage(ErrorMessages.Validation.EmailRequired + "*");

            _mockUserRepository.Verify(x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_EmptyUsername_ThrowsArgumentException()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = "";
            var displayName = _faker.Person.FullName;
            var password = _faker.Internet.Password(12);

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(email, username, displayName, password, _ct);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage(ErrorMessages.Validation.UsernameRequired + "*");

            _mockUserRepository.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_NullPassword_ThrowsArgumentException()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var username = _faker.Internet.UserName();
            string? password = null;
            var displayName = _faker.Person.FullName;

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(email, username, displayName, password!, _ct);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage(ErrorMessages.Validation.PasswordRequired + "*");

            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
