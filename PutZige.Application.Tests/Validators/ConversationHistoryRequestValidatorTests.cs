#nullable enable
using System;
using FluentAssertions;
using PutZige.Application.DTOs.Messaging;
using PutZige.Application.Validators.Messaging;
using Xunit;

namespace PutZige.Application.Tests.Validators;

public class ConversationHistoryRequestValidatorTests
{
    private readonly ConversationHistoryRequestValidator _validator = new ConversationHistoryRequestValidator();

    /// <summary>
    /// Verifies that Validator_ValidRequest_Passes behaves as expected.
    /// </summary>
    [Fact]
    public void Validator_ValidRequest_Passes()
    {
        var req = new ConversationHistoryRequest { OtherUserId = Guid.NewGuid(), PageNumber = 1, PageSize = 50 };
        var res = _validator.Validate(req);
        res.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Validator_OtherUserIdEmpty_Fails behaves as expected.
    /// </summary>
    [Fact]
    public void Validator_OtherUserIdEmpty_Fails()
    {
        var req = new ConversationHistoryRequest { OtherUserId = Guid.Empty, PageNumber = 1, PageSize = 50 };
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Validator_PageNumberZero_Fails behaves as expected.
    /// </summary>
    [Fact]
    public void Validator_PageNumberZero_Fails()
    {
        var req = new ConversationHistoryRequest { OtherUserId = Guid.NewGuid(), PageNumber = 0, PageSize = 10 };
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Validator_PageNumberNegative_Fails behaves as expected.
    /// </summary>
    [Fact]
    public void Validator_PageNumberNegative_Fails()
    {
        var req = new ConversationHistoryRequest { OtherUserId = Guid.NewGuid(), PageNumber = -1, PageSize = 10 };
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Validator_PageSizeZero_Fails behaves as expected.
    /// </summary>
    [Fact]
    public void Validator_PageSizeZero_Fails()
    {
        var req = new ConversationHistoryRequest { OtherUserId = Guid.NewGuid(), PageNumber = 1, PageSize = 0 };
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Validator_PageSizeNegative_Fails behaves as expected.
    /// </summary>
    [Fact]
    public void Validator_PageSizeNegative_Fails()
    {
        var req = new ConversationHistoryRequest { OtherUserId = Guid.NewGuid(), PageNumber = 1, PageSize = -5 };
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Validator_PageSizeExceedsMax_Fails behaves as expected.
    /// </summary>
    [Fact]
    public void Validator_PageSizeExceedsMax_Fails()
    {
        var req = new ConversationHistoryRequest { OtherUserId = Guid.NewGuid(), PageNumber = 1, PageSize = PutZige.Application.Common.Constants.AppConstants.Messaging.MaxPageSize + 1 };
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }
}
