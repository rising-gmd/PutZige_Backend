#nullable enable
using System;
using FluentAssertions;
using PutZige.Application.DTOs.Messaging;
using PutZige.Application.Validators.Messaging;
using Xunit;

namespace PutZige.Application.Tests.Validators;

public class SendMessageRequestValidatorTests
{
    private readonly SendMessageRequestValidator _validator = new SendMessageRequestValidator();

    [Fact]
    public void Validator_ValidRequest_Passes()
    {
        var req = new SendMessageRequest(Guid.NewGuid(), "hello");
        var res = _validator.Validate(req);
        res.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ReceiverIdEmpty_Fails()
    {
        var req = new SendMessageRequest(Guid.Empty, "hello");
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_MessageTextEmpty_Fails()
    {
        var req = new SendMessageRequest(Guid.NewGuid(), "");
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_MessageTextNull_Fails()
    {
        // record requires non-null; simulate via cast from null
        var req = new SendMessageRequest(Guid.NewGuid(), null!);
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_MessageTextExceedsMaxLength_Fails()
    {
        var tooLong = new string('x', PutZige.Application.Common.Constants.AppConstants.Messaging.MaxMessageLength + 1);
        var req = new SendMessageRequest(Guid.NewGuid(), tooLong);
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_MessageTextWhitespaceOnly_Fails()
    {
        var req = new SendMessageRequest(Guid.NewGuid(), "   ");
        var res = _validator.Validate(req);
        res.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_MessageTextExactlyMaxLength_Passes()
    {
        var exact = new string('x', PutZige.Application.Common.Constants.AppConstants.Messaging.MaxMessageLength);
        var req = new SendMessageRequest(Guid.NewGuid(), exact);
        var res = _validator.Validate(req);
        res.IsValid.Should().BeTrue();
    }
}
