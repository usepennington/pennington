# P0: YAML Deserialization Type Restrictions ✅ DONE

## Problem
`FrontMatterParser` (`src/Penn/FrontMatter/FrontMatterParser.cs`) creates a `DeserializerBuilder` with no type restrictions. YamlDotNet's default configuration can instantiate arbitrary types via YAML tags (e.g., `!<tag:yaml.org,2002:object>`), which is a deserialization attack vector if untrusted YAML is ever processed.

## Current State
- `DeserializerBuilder` uses `.WithNamingConvention(CamelCaseNamingConvention.Instance)` and `.IgnoreUnmatchedProperties()` only
- No `.DisableAliases()`, no type inspectors restricting allowed types
- The parser is used for both inline front matter and sidecar `.yml` files via `DeserializeYaml<T>()`

## Requirements
- Configure the deserializer to only allow instantiation of types that implement `IFrontMatter` and their property types (primitives, strings, arrays, DateTime, etc.)
- Disable YAML aliases to prevent billion-laughs-style expansion attacks
- Add unit tests with malicious YAML payloads: type tags attempting arbitrary instantiation, alias bombs, and unexpected object graphs
- The fix must not break any existing front matter types (`DocFrontMatter`, `BlogFrontMatter`, or consumer-defined types implementing `IFrontMatter`)

## Key Files
- `src/Penn/FrontMatter/FrontMatterParser.cs` — the deserializer to harden
- `src/Penn/FrontMatter/IFrontMatter.cs` — base interface
- `src/Penn/FrontMatter/Capabilities.cs` — capability interfaces with primitive properties
- `tests/Penn.Tests/` — add security-focused tests
