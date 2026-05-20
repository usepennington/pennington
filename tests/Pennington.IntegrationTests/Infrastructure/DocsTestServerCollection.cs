namespace Pennington.IntegrationTests.Infrastructure;

/// <summary>
/// Collection definition for tests that share a <see cref="DocsWebApplicationFactory"/>.
/// Multiple WebApplicationFactory instances built on the same TEntryPoint conflict
/// when run in parallel — they share static caching for the underlying TestServer
/// and dispose each other's state. Tests that consume the factory must opt into
/// this collection so xUnit runs them sequentially with a single shared instance.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DocsTestServerCollection : ICollectionFixture<DocsWebApplicationFactory>
{
    public const string Name = "DocsTestServer";
}