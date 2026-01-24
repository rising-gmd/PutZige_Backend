#nullable enable
using System;
using FluentAssertions;
using FluentValidation.Results;
using PutZige.Application.Settings;
using PutZige.Application.Validators;
using Xunit;

namespace PutZige.Application.Tests.Validators
{
    public class HashingSettingsValidatorTests
    {
        private readonly HashingSettingsValidator _validator = new();

        /// <summary>
        /// Valid hashing settings pass validation.
        /// </summary>
        [Fact]
        public void Validate_ValidSettings_PassesValidation()
        {
            var settings = new HashingSettings { SaltSizeBytes = 32, Algorithm = "SHA512", Iterations = 100000 };
            var result = _validator.Validate(settings);
            result.IsValid.Should().BeTrue();
        }

        /// <summary>
        /// Salt size below minimum fails validation.
        /// </summary>
        [Fact]
        public void Validate_SaltSizeTooSmall_FailsValidation()
        {
            var settings = new HashingSettings { SaltSizeBytes = 8, Algorithm = "SHA512", Iterations = 100000 };
            var result = _validator.Validate(settings);
            result.IsValid.Should().BeFalse();
        }

        /// <summary>
        /// Salt size above maximum fails validation.
        /// </summary>
        [Fact]
        public void Validate_SaltSizeTooLarge_FailsValidation()
        {
            var settings = new HashingSettings { SaltSizeBytes = 128, Algorithm = "SHA512", Iterations = 100000 };
            var result = _validator.Validate(settings);
            result.IsValid.Should().BeFalse();
        }

        /// <summary>
        /// Unsupported algorithm fails validation.
        /// </summary>
        [Fact]
        public void Validate_InvalidAlgorithm_FailsValidation()
        {
            var settings = new HashingSettings { SaltSizeBytes = 32, Algorithm = "MD5", Iterations = 100000 };
            var result = _validator.Validate(settings);
            result.IsValid.Should().BeFalse();
        }

        /// <summary>
        /// Iteration count below minimum fails validation.
        /// </summary>
        [Fact]
        public void Validate_IterationsTooLow_FailsValidation()
        {
            var settings = new HashingSettings { SaltSizeBytes = 32, Algorithm = "SHA512", Iterations = 5000 };
            var result = _validator.Validate(settings);
            result.IsValid.Should().BeFalse();
        }

        /// <summary>
        /// Iteration count above maximum fails validation.
        /// </summary>
        [Fact]
        public void Validate_IterationsTooHigh_FailsValidation()
        {
            var settings = new HashingSettings { SaltSizeBytes = 32, Algorithm = "SHA512", Iterations = 2000000 };
            var result = _validator.Validate(settings);
            result.IsValid.Should().BeFalse();
        }

        /// <summary>
        /// Empty algorithm string fails validation.
        /// </summary>
        [Fact]
        public void Validate_EmptyAlgorithm_FailsValidation()
        {
            var settings = new HashingSettings { SaltSizeBytes = 32, Algorithm = "", Iterations = 100000 };
            var result = _validator.Validate(settings);
            result.IsValid.Should().BeFalse();
        }
    }
}
