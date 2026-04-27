# tests/ — Test conventions

## Framework & style
- xunit.v3 with Shouldly assertions (`value.ShouldBe(...)`, `collection.ShouldContain(...)`).
- Test classes named `<ClassUnderTest>Tests`. No shared fixtures across unit tests — each test builds its own state.
- Any API overload that takes a `CancellationToken` must receive `TestContext.Current.CancellationToken` (xUnit1051). Don't leave the default — the analyzer treats it as a warning, and tests should cancel responsively.

## Unit tests (`Pennington.Tests`)
- Tests use a `CreateTestService(...)` factory pattern to assemble the service under test with a `Testably.Abstractions` `MockFileSystem`.
- Fake markdown (with YAML front matter) is written inline in the test body, not from disk fixtures — keeps each test self-contained and diffable.

## Integration tests (`Pennington.IntegrationTests`)
- `DocsWebApplicationFactory` + `DocsTestServerCollection` is the canonical fixture pattern. Uses `WebApplicationFactory<Program>` (TestServer in-process) so self-fetching services (LlmsTxtService, SearchIndexService) flow through the same pipeline that serves browser requests — `IInProcessHttpDispatcher` detects the TestServer and dispatches in-memory.
- Environment name is `"Testing"`; logs are clamped to `Warning` to keep output clean.
- The docs project path is resolved by a relative walkup from the test binary — don't hardcode absolute paths.
