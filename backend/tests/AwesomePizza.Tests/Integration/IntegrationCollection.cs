namespace AwesomePizza.Tests.Integration;

// All the integration tests share one database, so they run one at a time
[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Integration";
}
