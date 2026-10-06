using Bogus;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Testing.Shared.Builders;

public sealed class UserBuilder
{
    private readonly Faker _faker = new();
    private Guid _tenantId = Guid.NewGuid();
    private string? _email;
    private string _passwordHash = "$2a$11$e8k8Wk6oOqFf3FfVz8S2zO2x2dY6/QYpI9x8N5aBq8eXlSgC7H6bO"; // BCrypt hash of "P@ssword123!"
    private string? _firstName;
    private string? _lastName;

    public UserBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public User Build()
    {
        var email = _email ?? _faker.Internet.Email();
        var first = _firstName ?? _faker.Name.FirstName();
        var last = _lastName ?? _faker.Name.LastName();

        return new User(_tenantId, email, _passwordHash, first, last);
    }
}
