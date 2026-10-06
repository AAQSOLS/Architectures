using ModularSaaS.Testing.Shared.Fixtures;
using Xunit;

namespace ModularSaaS.Integration.Tests.Collections;

[CollectionDefinition("Database")]
public class DatabaseTestCollection : ICollectionFixture<MsSqlDatabaseFixture>
{
}
