using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity;

public class Permission : BaseEntity
{
    private Permission()
    {
    }

    public Permission(string code, string name, string module, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(module);

        Code = code.Trim();
        Name = name.Trim();
        Module = module.Trim();
        Description = description.Trim();
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Module { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;
}
