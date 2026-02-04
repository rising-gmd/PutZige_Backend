#nullable enable
using System;
using FluentAssertions;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.Validators;
using Xunit;

namespace PutZige.Application.Tests.Validators
{
    public class EmailVerificationValidatorTests
    {
        private readonly VerifyEmailRequestValidator _verify = new();
        private readonly ResendVerificationRequestValidator _resend = new();

        [Fact]
        public void VerifyEmailRequestValidator_ValidRequest_PassesValidation()
        {
            var req = new VerifyEmailRequest("user@ex.com", "token123");
            var result = _verify.Validate(req);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void VerifyEmailRequestValidator_NullEmail_FailsValidation()
        {
            var req = new VerifyEmailRequest(null!, "token");
            var result = _verify.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void VerifyEmailRequestValidator_EmptyEmail_FailsValidation()
        {
            var req = new VerifyEmailRequest("", "token");
            var result = _verify.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void VerifyEmailRequestValidator_InvalidEmailFormat_FailsValidation()
        {
            var req = new VerifyEmailRequest("not-an-email", "token");
            var result = _verify.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void VerifyEmailRequestValidator_NullToken_FailsValidation()
        {
            var req = new VerifyEmailRequest("user@ex.com", null!);
            var result = _verify.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void VerifyEmailRequestValidator_EmptyToken_FailsValidation()
        {
            var req = new VerifyEmailRequest("user@ex.com", "");
            var result = _verify.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void VerifyEmailRequestValidator_WhitespaceToken_FailsValidation()
        {
            var req = new VerifyEmailRequest("user@ex.com", "   ");
            var result = _verify.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ResendVerificationRequestValidator_ValidRequest_PassesValidation()
        {
            var req = new ResendVerificationRequest("user@ex.com");
            var result = _resend.Validate(req);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ResendVerificationRequestValidator_NullEmail_FailsValidation()
        {
            var req = new ResendVerificationRequest(null!);
            var result = _resend.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ResendVerificationRequestValidator_EmptyEmail_FailsValidation()
        {
            var req = new ResendVerificationRequest("");
            var result = _resend.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ResendVerificationRequestValidator_InvalidEmailFormat_FailsValidation()
        {
            var req = new ResendVerificationRequest("not-an-email");
            var result = _resend.Validate(req);
            result.IsValid.Should().BeFalse();
        }
    }
}
