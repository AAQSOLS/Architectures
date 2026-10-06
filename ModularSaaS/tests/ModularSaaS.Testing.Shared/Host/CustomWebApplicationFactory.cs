using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Testing.Shared.Fixtures;
using ModularSaaS.Testing.Shared.Host.Fakes;

namespace ModularSaaS.Testing.Shared.Host;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MsSqlDatabaseFixture _databaseFixture;
    public FakeEmailSender FakeEmailSender { get; } = new();

    public CustomWebApplicationFactory(MsSqlDatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    public string ConnectionString => _databaseFixture.ConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _databaseFixture.ConnectionString,
                ["Outbox:PollingIntervalSeconds"] = "60",
                ["Seed:PlatformAdminPassword"] = "P@ssword123!",
                ["Seed:DefaultTenantAdminPassword"] = "P@ssword123!"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace external email sender with fake
            var emailDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (emailDescriptor != null)
            {
                services.Remove(emailDescriptor);
            }
            services.AddSingleton<IEmailSender>(FakeEmailSender);
        });
    }
}
