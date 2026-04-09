---
title: "Linking and Cross-References"
description: "Link between documents using relative paths, reference media assets, and use xref:uid syntax for type-safe cross-references that survive URL changes"
uid: "penn.how-to.linking-and-cross-references"
order: 20
---

## Beat 1: Relative Links Between Sibling Pages

Create two markdown pages in the same directory and link between them with standard markdown link syntax. Establish that relative links are the simplest and most common approach.

### What to show
- Two files in `Content/getting-started/`: `index.md` and `configuration.md`, each with front matter parsed by `T:Pennington.FrontMatter.FrontMatterParser`
- Standard markdown link syntax: `[Configuration](configuration.md)` in `index.md` and `[Getting Started](index.md)` in `configuration.md`
- Explain how `M:Pennington.Routing.ContentRouteFactory.FromMarkdownFile(Pennington.Routing.FilePath,Pennington.Routing.FilePath,Pennington.Routing.UrlPath,System.String)` converts file paths to URL paths, so `configuration.md` becomes `/getting-started/configuration/`
- The link is resolved by the browser at the resulting URL; Pennington does not rewrite relative markdown links

### Key points
- Relative links work for pages in the same directory or nearby directories
- They break when pages move to different directories, which motivates the xref system introduced later
- `T:Pennington.Routing.ContentRoute` tracks both `P:Pennington.Routing.ContentRoute.CanonicalPath` (the URL) and `P:Pennington.Routing.ContentRoute.OutputFile` (the generated file path)

## Beat 2: Media Links (Images and Assets)

Add an architecture diagram image to the content directory and reference it from markdown. Show that Pennington copies non-markdown files to the output.

### What to show
- Place `beacon-arch.png` alongside the markdown files in `Content/getting-started/`
- Reference with `![Beacon architecture](./beacon-arch.png)` in `index.md`
- Explain that `M:Pennington.Content.MarkdownContentService`1.GetContentToCopyAsync` discovers non-markdown files (excluding `.md`, `.mdx`, `.razor`, `.yml`, `.yaml`) and copies them to the output directory, preserving relative paths
- The returned `T:Pennington.Content.ContentToCopy` records pair source paths with output paths

### Key points
- Media files must live within the content directory tree to be discovered and copied
- The relative path from the markdown file to the image is preserved in the output, so the link works without rewriting
- PDFs, downloads, and other binary assets follow the same copy mechanism

## Beat 3: Absolute and External Links

Link to an external URL and discuss the pitfalls of internal absolute links when deploying to subdirectories.

### What to show
- An external link: `[Beacon on GitHub](https://github.com/example/beacon)` -- standard markdown, no Pennington processing
- An internal absolute link: `[Home](/)`  -- works at the root but breaks if the site is deployed under a subdirectory like `/docs/`
- Reference `M:Pennington.Routing.ContentRoute.WithBaseUrl(Pennington.Routing.UrlPath)` which prepends a base URL to a route's canonical path, showing how Pennington handles subdirectory deployment at the routing level
- The build's link verification via `T:Pennington.Infrastructure.LinkVerificationService` checks internal links for validity

### Key points
- External links pass through unchanged; Pennington does not proxy or validate external URLs
- For internal cross-page links, prefer relative links or xref over absolute paths
- `T:Pennington.Generation.BrokenLink` records track link type via `T:Pennington.Generation.LinkType` (`Internal`, `External`, `Anchor`, `Image`) with a `Reason` string

## Beat 4: Assign UIDs for Cross-Referencing

Add `uid` values to page front matter to enable type-safe cross-references that survive URL changes.

### What to show
- Add `uid: "beacon.api-reference"` to the API reference page and `uid: "beacon.getting-started"` to the getting-started page in their YAML front matter
- Reference `T:Pennington.FrontMatter.ICrossReferenceable` which defines `P:Pennington.FrontMatter.ICrossReferenceable.Uid` -- the single-property interface that opts a front matter type into the xref system
- Show that `T:Pennington.FrontMatter.DocFrontMatter` already implements `ICrossReferenceable` (along with `IDraftable`, `ITaggable`, `ISectionable`, `IOrderable`, `IDescribable`)
- Show that `T:Pennington.DocSite.DocSiteFrontMatter` also implements `ICrossReferenceable` -- this is the front matter type used by the DocSite template
- Reference `T:Pennington.Pipeline.CrossReference` record: `CrossReference(string Uid, string Title, ContentRoute Route)` -- the resolved triple stored in the lookup

### Key points
- UIDs are arbitrary strings; by convention use dotted namespaces like `beacon.api-reference`
- UIDs must be unique across all content sources; the last registration wins (case-insensitive)
- `M:Pennington.Content.MarkdownContentService`1.GetCrossReferencesAsync` collects UIDs from all pages where `fm is ICrossReferenceable { Uid: { } uid }`

## Beat 5: Use xref Links

Write cross-reference links using the `xref:uid` syntax in both inline tag form and standard link form. Show both resolution paths.

### What to show
- Inline tag syntax: `<xref:beacon.api-reference>` in markdown, which the `T:Pennington.Infrastructure.XrefResolvingService` resolves via regex `<xref:([^>]+)>` (see `XrefResolvingService.XrefTagRegex`)
- Link href syntax: `[API Reference](xref:beacon.api-reference)` in markdown, which renders as `<a href="xref:beacon.api-reference">` and is resolved by AngleSharp DOM query `a[href^='xref:']`
- Reference the two-phase resolution in `M:Pennington.Infrastructure.XrefResolvingService.ResolveAsync(System.String,Pennington.Diagnostics.DiagnosticContext)`: Phase 1 resolves `<xref:uid>` raw tags via string replacement; Phase 2 parses with AngleSharp and resolves `<a href="xref:...">` links
- Reference `T:Pennington.Infrastructure.XrefResolver` which builds a case-insensitive `ImmutableDictionary<string, CrossReference>` lookup from all `T:Pennington.Content.IContentService` instances via `M:Pennington.Content.IContentService.GetCrossReferencesAsync`
- The resolved link replaces the href with `xref.Route.CanonicalPath.Value` and (for inline tags) uses `xref.Title` as the link text

### Key points
- Resolution is case-insensitive: `xref:Beacon.API-Reference` resolves the same as `xref:beacon.api-reference`
- The `XrefResolver` is lazily built on first use and rebuilt when files change (via `FileWatchDependencyFactory`)
- `T:Pennington.Infrastructure.XrefResolvingProcessor` is an `T:Pennington.Infrastructure.IResponseProcessor` that runs in the response pipeline (`P:Pennington.Infrastructure.XrefResolvingProcessor.Order` is -10, so it runs early)

## Beat 6: Move a Page and See xrefs Survive

Rename the API reference directory from `api/` to `reference/`. Demonstrate that xref links still resolve while relative links would break.

### What to show
- Move `Content/api/index.md` to `Content/reference/index.md` -- the `uid: "beacon.api-reference"` stays the same
- `ContentRouteFactory.FromMarkdownFile` now generates a new `CanonicalPath` of `/reference/` instead of `/api/`, but the `CrossReference` record updates its `Route` automatically because `GetCrossReferencesAsync` re-scans on file changes
- The xref link `xref:beacon.api-reference` in the getting-started page still resolves because the `XrefResolver` lookup is keyed by UID, not by path
- A hypothetical relative link `[API Reference](../api/index.md)` would now produce a 404

### Key points
- UIDs decouple link targets from filesystem structure -- this is the primary value proposition of xrefs
- The `XrefResolver` rebuilds its lookup lazily, so changes are picked up on the next request in dev mode
- In static builds, the lookup is built once at generation time from all content services

## Beat 7: Detect Broken xrefs with Build Diagnostics

Intentionally typo a UID and show how broken xrefs surface as diagnostics in the build output.

### What to show
- Change a link to `xref:beacon.api-refrence` (typo) and run the build
- In `XrefResolvingService.ResolveXrefTagsAsync` and `ResolveXrefLinksAsync`, when `XrefResolver.ResolveAsync` returns null, the service calls `M:Pennington.Diagnostics.DiagnosticContext.AddWarning(System.String,System.String)` with message `"Unresolved xref: beacon.api-refrence"` and source `"XrefResolver"`
- The unresolved link gets `data-xref-error="Reference not found"` and `data-xref-uid` attributes in the HTML output, making broken xrefs visible in the rendered page
- Reference `T:Pennington.Diagnostics.DiagnosticContext` (scoped per-request) which accumulates `T:Pennington.Diagnostics.Diagnostic` records with `T:Pennington.Diagnostics.DiagnosticSeverity` (`Info`, `Warning`, `Error`)
- In static builds, diagnostics are propagated via `X-Pennington-Diagnostic` response headers (parsed by `OutputGenerationService.ParseDiagnosticHeaders`) and collected into `T:Pennington.Generation.BuildReport` via `T:Pennington.Generation.BuildDiagnostic`
- Reference `P:Pennington.Generation.BuildReport.Diagnostics` and `M:Pennington.Generation.BuildReport.WriteTo(System.IO.TextWriter)` which formats the report with warnings section showing unresolved xrefs
- Reference `P:Pennington.Generation.BuildReport.HasErrors` which checks for error-severity diagnostics or broken links

### Key points
- Broken xrefs are warnings, not errors -- the build still succeeds but the report highlights them
- The `DiagnosticOverlayProcessor` can show diagnostics in a dev-mode overlay for immediate feedback
- `T:Pennington.Generation.BrokenLink` records (from `LinkVerificationService`) track a different category of issues -- internal links that point to non-existent routes -- and appear separately in `P:Pennington.Generation.BuildReport.BrokenLinks`
