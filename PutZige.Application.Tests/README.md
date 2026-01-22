# PutZige.Application.Tests - Service & Application Logic Tests

## What We Test

- Application services (e.g. `UserService`) business logic
- Validation logic (FluentValidation) and mapping behavior
- Command/Query handlers and transactional flows
- NO real database (mock dependencies instead)

## Key Tools

- `Moq` - for mocking `IUserRepository`, `IUnitOfWork`, `IMapper`
- `FluentAssertions` - expressive assertions
- `Bogus` - realistic fake data for tests
- `xUnit` - test runner

## Principles

- Mock all external dependencies (repositories, mappers, units of work)
- Use Arrange-Act-Assert (AAA) pattern
- Verify mock interactions using `Verify()`
- Test happy paths and error paths (validation, exceptions)

## Project Structure

- `Services/` - tests for application services
- `Validators/` - validation tests
- `Handlers/` - command/query handler tests
- `Builders/` - test data builders

## Example: `UserService` Test with Mocks

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using Bogus;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_WhenValid_CallsRepositoryAndReturnsId()
    {
        // Arrange
        var faker = new Faker();
        var dto = new CreateUserDto { Email = faker.Internet.Email(), Name = faker.Person.FullName };

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(new User("id", dto.Email));

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<User>(It.IsAny<CreateUserDto>()))
              .Returns((CreateUserDto d) => new User("id", d.Email));

        var service = new UserService(userRepo.Object, uow.Object, mapper.Object);

        // Act
        var result = await service.CreateUserAsync(dto);

        // Assert
        result.Should().NotBeNull();
        userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
```

## Running Tests

```
dotnet test PutZige.Application.Tests
```

Run in watch mode for TDD:

```
dotnet watch test --project PutZige.Application.Tests
```

## Best Practices

- Mock all external dependencies
- Keep tests deterministic
- Verify interactions where behavior is important
- Use `Bogus` to produce realistic but deterministic data (set seed when necessary)

- Don't use real DBs here

## Common Patterns

- Use `AutoFixture` or lightweight `Builders` for DTO setup
- Use `Theory` + `InlineData` for different validation scenarios
- Use `Callback` on mocks to capture arguments for deeper assertions

## Example: Validator Test

```csharp
using Xunit;
using FluentValidation.TestHelper;

public class CreateUserDtoValidatorTests
{
    [Fact]
    public void Validator_rejects_empty_email()
    {
        var validator = new CreateUserDtoValidator();
        var dto = new CreateUserDto { Email = "", Name = "John" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
```

## Troubleshooting

- If a test fails due to mock setup, verify `It.IsAny<T>()` vs concrete values
- Use `MockBehavior.Strict` sparingly for contract-style tests
- Ensure async mocks return `Task`/`Task<T>` correctly

## Quick Checklist

- [ ] All external dependencies mocked
- [ ] Tests cover happy + edge cases
- [ ] No I/O or DB access

## Summary

This project validates application-layer behavior with mocked dependencies. Tests here are slightly slower than domain tests but remain unit tests. Focus on service coordination, validation, and mapping correctness.
