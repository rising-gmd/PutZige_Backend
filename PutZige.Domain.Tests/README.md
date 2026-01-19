# PutZige.Domain.Tests - Pure Domain Logic Tests

## What We Test

- Entity business rules
- Value object behavior and invariants
- Domain events and simple factories
- NO database, NO HTTP, NO external dependencies

## Purpose

This project contains fast, deterministic unit tests for pure domain logic. Tests should be small, independent, and free of any I/O. Keep tests focused on business rules and value object invariants.

## Current State

The `User` entity is a simple POCO today, so tests are minimal. As domain logic grows (validation, invariants, computed properties), add tests here.

## Test Guidelines

- Keep tests small and explicit
- Use Arrange-Act-Assert
- No mocking frameworks required
- Prefer parameterized tests for boundary cases
- Name tests using the "When_Then" convention

## Project Structure

- `Entities/` - entity tests
- `ValueObjects/` - value object tests
- `Rules/` - business rule tests
- `Builders/` - small builders for test data

## Example: Simple Entity Validation Test

```csharp
// ...example test file...
using Xunit;
using FluentAssertions;

public class UserTests
{
    [Fact]
    public void When_creating_user_with_empty_email_then_invalid()
    {
        // Arrange
        var email = string.Empty;

        // Act
        Action act = () => new User("id", email);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Email is required");
    }
}
```

## How To Add Tests

1. Add a test file under the appropriate folder.
2. Write focused tests that assert domain invariants.
3. Avoid test-level fixtures with external dependencies.

## Running Tests

Run tests locally:

```
dotnet test PutZige.Domain.Tests
```

Use filtering to run specific tests:

```
dotnet test --filter "FullyQualifiedName~UserTests"
```

## Best Practices

- Test domain logic only
- Use value objects for encapsulated invariants
- Keep tests deterministic and fast

- Don't reach for mocks or integration tools here

## Common Patterns

- Use small builders to reduce duplication
- Use `[Theory]` + `InlineData` for boundary checks
- Use `FluentAssertions` for readable assertions

## Example: Value Object Tests

```csharp
using Xunit;
using FluentAssertions;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("x@y.io")]
    public void Valid_emails_are_accepted(string value)
    {
        var email = Email.Create(value);
        email.Value.Should().Be(value);
    }

    [Fact]
    public void Invalid_email_throws()
    {
        Action act = () => Email.Create("not-an-email");
        act.Should().Throw<FormatException>();
    }
}
```

## When To Expand This Project

- Add domain services tests when logic lives in the domain
- Add more value object tests as new VOs are introduced
- Keep tests focused: if a test touches infrastructure, move it elsewhere

## Quick Checklist

- [ ] Tests are pure: no I/O
- [ ] Tests run < 100ms each
- [ ] No mocking libraries referenced

## Troubleshooting

- If tests become slow, check for accidental static/global state
- If a test needs a DB or HTTP, move it to Integration tests in `API.Tests` or `Infrastructure.Tests`

## Helpful Commands

```
dotnet test PutZige.Domain.Tests
```

## Summary

This project is the first line of defense for domain correctness. Keep it small, fast, and focused on business invariants. Tests here should be easy to read and change alongside domain model updates.
