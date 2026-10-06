using System.Net.Http.Headers;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Testing.Shared.Security;

namespace ModularSaaS.Testing.Shared.Extensions;

public static class HttpClientAuthExtensions
{
    public static HttpClient AsTenantUser(
        this HttpClient client,
        Guid tenantId,
        Guid userId,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? roles = null,
        string email = "testuser@example.com")
    {
        var token = TestJwtTokenGenerator.GenerateUserToken(
            tenantId: tenantId,
            userId: userId,
            email: email,
            roles: roles,
            permissions: permissions);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Remove(AppHeaders.TenantId);
        client.DefaultRequestHeaders.Add(AppHeaders.TenantId, tenantId.ToString());

        return client;
    }

    public static HttpClient AsPlatformAdmin(
        this HttpClient client,
        Guid? userId = null,
        string email = "platformadmin@example.com")
    {
        var adminId = userId ?? Guid.NewGuid();
        var token = TestJwtTokenGenerator.GeneratePlatformToken(adminId, email);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Remove(AppHeaders.TenantId);

        return client;
    }

    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Remove(AppHeaders.TenantId);
        return client;
    }
}
