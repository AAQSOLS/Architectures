namespace ModularSaaS.Security.Authorization;

public interface IPermissionEvaluator
{
    public ValueTask<bool> HasPermissionAsync(string permission, CancellationToken ct = default);

    public ValueTask<bool> HasAnyPermissionAsync(IReadOnlyCollection<string> permissions, CancellationToken ct = default);

    public ValueTask<bool> HasAllPermissionsAsync(IReadOnlyCollection<string> permissions, CancellationToken ct = default);
}
