# P0: Path Traversal Validation in ContentRouteFactory

## Problem
`ContentRouteFactory.FromMarkdownFile()` (`src/Pennington/Routing/ContentRouteFactory.cs`) does not validate that the resolved file path stays within the content root directory. The `GetRelativePath` helper normalizes slashes but doesn't prevent `../../` sequences from escaping the content boundary.

## Current State
- `GetRelativePath()` (line 107) does simple string prefix matching after slash normalization
- If the prefix check fails, it returns the full normalized path unchanged (line 117) — no error, no boundary enforcement
- `FromMarkdownFile` is called during content discovery in `MarkdownContentService.DiscoverAsync()`, which uses file system enumeration (lower risk), but the method is public and could be called with arbitrary paths

## Requirements
- After computing the relative path, validate that `Path.GetFullPath(sourceFile)` starts with `Path.GetFullPath(contentRoot)` — reject with a clear exception if not
- This validation should apply to `FromMarkdownFile` since it's the only method that accepts file paths; the other factory methods (`FromUrl`, `FromRazorPage`, `FromCustom`) work with URL paths and don't have this risk
- Add unit tests with path traversal payloads: `../../etc/passwd`, `Content\..\..\secret`, and paths with encoded separators
- Ensure the validation works correctly on both Windows (backslash) and Unix (forward slash) paths

## Key Files
- `src/Pennington/Routing/ContentRouteFactory.cs` — add validation in `FromMarkdownFile`
- `tests/Pennington.Tests/` — add security-focused tests
