using FluentAssertions;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Identity.Validators;
using Xunit;

namespace ModularSaaS.Unit.Tests.Application.Identity.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_PassesValidation()
    {
        var input = new RegisterUserInput("user@example.com", "P@ssword123!", "John", "Doe");

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    public void Validate_WithInvalidEmail_FailsValidation(string email)
    {
        var input = new RegisterUserInput(email, "P@ssword123!", "John", "Doe");

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserInput.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_WithShortPassword_FailsValidation(string password)
    {
        var input = new RegisterUserInput("user@example.com", password, "John", "Doe");

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserInput.Password));
    }

    [Fact]
    public void Validate_WithEmptyNames_FailsValidation()
    {
        var input = new RegisterUserInput("user@example.com", "P@ssword123!", "", "");

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserInput.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserInput.LastName));
    }
}
