# P3: Documentation Versioning Support

## Problem
Penn has no concept of content versions. API documentation sites commonly need to serve multiple versions simultaneously (e.g., `/v1/docs/`, `/v2/docs/`, `/latest/docs/`) so users on older releases can access the correct documentation.

## Current State
- `ContentRoute` has `CanonicalPath`, `OutputFile`, `SourceFile`, `Locale` — no version field
- `MarkdownContentOptions` has `ContentPath`, `BasePageUrl`, `Section` — no version configuration
- `PennOptions.AddMarkdownContent<T>()` registers content sources but has no version awareness
- Navigation is built per-site, not per-version
- No version selector UI component exists
- The locale system (`/{locale}/` URL prefix) provides a pattern that versioning could follow

## Requirements

### Version Configuration
- Add a `Version` property to `MarkdownContentOptions` so a content source can declare its version
- When a version is set, prefix the content URLs with the version (e.g., `BasePageUrl="/docs"` + `Version="v2"` = `/v2/docs/...`)
- Support a "latest" alias that maps to a specific version
- Multiple versions of the same content source can be registered (same `ContentPath` with different `BasePageUrl` or content directories per version)

### Version-Aware Routing
- Add a `Version` property to `ContentRoute` (similar to `Locale`)
- `ContentRouteFactory.FromMarkdownFile` should accept an optional version parameter
- Version and locale can coexist: `/{locale}/{version}/docs/...`

### Version Selector UI
- Create a `VersionSelector` component (similar pattern to `LanguageSwitcher`) that renders a dropdown/links for available versions
- The component should navigate to the same page in a different version if it exists, or to the version's root if not
- Place in the doc site header or sidebar

### Navigation Per Version
- Each version should have its own navigation tree — content may differ between versions
- `NavigationBuilder` already builds from TOC entries scoped to content services, so separate versioned content services should produce separate trees

### Static Build
- All versions are built simultaneously — each version's pages are separate routes
- Sitemap should include all versioned pages

## Key Files
- `src/Penn/Infrastructure/PennOptions.cs` — `MarkdownContentOptions` needs `Version`
- `src/Penn/Routing/ContentRoute.cs` — add `Version` property
- `src/Penn/Routing/ContentRouteFactory.cs` — version-aware URL generation
- `src/Penn/Content/MarkdownContentService.cs` — pass version through discovery
- `src/Penn.UI/Components/` — new `VersionSelector` component
- `src/Penn.DocSite/Components/Layout/MainLayout.razor` — include version selector
- `src/Penn/Navigation/NavigationBuilder.cs` — version-scoped tree building
