# tests/ — Test conventions

## Framework & style
- xunit.v3 with Shouldly assertions (`value.ShouldBe(...)`, `collection.ShouldContain(...)`).
- Test classes named `<ClassUnderTest>Tests`. No shared fixtures across unit tests — each test builds its own state.

## Unit tests (`Pennington.Tests`)
- Tests use a `CreateTestService(...)` factory pattern to assemble the service under test with a `Testably.Abstractions` `MockFileSystem`.
- Fake markdown (with YAML front matter) is written inline in the test body, not from disk fixtures — keeps each test self-contained and diffable.

## Integration tests (`Pennington.IntegrationTests`)
- `DocsRealServerFixture` (`IAsyncLifetime`) in `Infrastructure/` spins real Kestrel on a random port rather than using `TestServer`. Pick it when a test needs to self-fetch via `HttpClient` (e.g., endpoints that issue their own HTTP calls back to the app).
- Environment name is `"Testing"`; logs are clamped to `Warning` to keep output clean.
- The docs project path is resolved by a relative walkup from the test binary — don't hardcode absolute paths.
