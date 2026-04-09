# P1: Front Matter Schema Validation

## Problem
Front matter is parsed with `IgnoreUnmatchedProperties()` and no field-level validation. Typos in property names are silently ignored, required fields can be empty strings, and there's no way for content authors to get feedback about invalid front matter during development.

## Current State
- `FrontMatterParser` deserializes YAML into types implementing `IFrontMatter` with no validation beyond what YamlDotNet provides (type coercion)
- `IFrontMatter` requires `string Title` but an empty string satisfies the interface
- Capability interfaces (`IDraftable`, `ITaggable`, etc.) have no validation attributes
- The `DiagnosticContext` system already exists for surfacing warnings/errors during development
- Content discovery happens in `MarkdownContentService.DiscoverAsync()` which yields `DiscoveredItem` — validation could run after parsing, before yielding

## Requirements
- Add a validation step after YAML deserialization that checks:
  - `Title` is not null or whitespace
  - Required properties on the front matter type are populated (use data annotation attributes like `[Required]` or a new `[FrontMatterRequired]` attribute)
  - Optional: format validation for dates, URLs, UIDs
- Report validation failures as diagnostics (warnings in dev mode, errors in build mode) rather than throwing exceptions — content with invalid front matter should still be processed but flagged
- The validation system should be extensible: consumers defining custom `IFrontMatter` types should be able to add their own validation attributes
- Add validation results to `DiagnosticContext` so they appear in the diagnostic overlay during development
- Add unit tests for: missing title, empty title, unrecognized properties (already ignored — verify diagnostic is emitted), invalid date formats

## Key Files
- `src/Pennington/FrontMatter/FrontMatterParser.cs` — add validation after deserialization
- `src/Pennington/FrontMatter/IFrontMatter.cs` — consider adding validation contract
- `src/Pennington/Diagnostics/DiagnosticContext.cs` — validation results go here
- `src/Pennington/Content/MarkdownContentService.cs` — wire validation into discovery pipeline
- `tests/Pennington.Tests/` — validation tests
