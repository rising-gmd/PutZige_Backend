#nullable enable
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using PutZige.Application.Validators.Auth;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.Common.Messages;

namespace PutZige.Application.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidData_PassesValidation()
        {
            var model = new LoginRequest { Email = "user@test.com", Password = "Password1!" };
            var result = await _validator.ValidateAsync(model);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_EmptyEmail_FailsValidation()
        {
            var model = new LoginRequest { Email = "", Password = "Password1!" };
            var result = await _validator.ValidateAsync(model);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Equals("email", System.StringComparison.OrdinalIgnoreCase) || e.ErrorMessage == ErrorMessages.Validation.EmailRequired);
        }

        [Fact]
        public async Task Validate_InvalidEmailFormat_FailsValidation()
        {
            var model = new LoginRequest { Email = "not-email", Password = "Password1!" };
            var result = await _validator.ValidateAsync(model);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Equals("email", System.StringComparison.OrdinalIgnoreCase) || e.ErrorMessage == ErrorMessages.Validation.EmailInvalidFormat);
        }

        [Fact]
        public async Task Validate_EmptyPassword_FailsValidation()
        {
            var model = new LoginRequest { Email = "user@test.com", Password = "" };
            var result = await _validator.ValidateAsync(model);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Equals("password", System.StringComparison.OrdinalIgnoreCase) || e.ErrorMessage == ErrorMessages.Validation.PasswordRequired);
        }
    }
}
