using ModularSaaS.Testing.Shared.Host;
using Xunit;

namespace ModularSaaS.Functional.Tests.Collections;

[CollectionDefinition("Api")]
public class ApiTestCollection : ICollectionFixture<ApiTestFixture>
{
}
