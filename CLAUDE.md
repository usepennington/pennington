# Penn

Content engine library targeting .NET 11 / C# 15 with union types.

## Build & Test
- Build: `dotnet build Penn.slnx`
- Test: `dotnet test Penn.slnx`
- Single test: `dotnet test Penn.slnx --filter "FullyQualifiedName~TestName"`

## Project Structure
- `src/Penn/` — Core library
- `tests/Penn.Tests/` — xUnit tests with FluentAssertions
- Union types require polyfills in `Infrastructure/UnionPolyfills.cs` until .NET 11 RTM

## Conventions
- C# 15 union types for discriminated unions (not abstract base classes)
- Records for data types
- ImmutableList for collection properties on public types
- Async methods return IAsyncEnumerable or Task
