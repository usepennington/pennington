---
title: "Implementing Redirects"
description: "Redirect moved pages using the IRedirectable capability and RedirectUrl front matter property, producing meta-refresh HTML in static builds"
uid: "penn.how-to.implementing-redirects"
order: 40
---

## Beat 1: The Problem -- Moved Pages and Broken Links

Set the scene: Beacon's docs have been reorganized, moving `/setup/` to `/getting-started/`. External links, bookmarks, and search engine results still point to the old URL.

### What to show
- The scenario: a documentation restructure where `Content/setup.md` has been replaced by `Content/getting-started.md` at a new URL
- Without a redirect, visitors to `/setup/` see a 404 and search engines lose their index entry
- Penn solves this with the `IRedirectable` capability interface and meta-refresh HTML generation

### Key points
- Redirects preserve SEO value by pointing crawlers to the new canonical URL
- Penn generates static HTML redirect files rather than relying on server-side redirect rules, making this work with any static hosting provider (GitHub Pages, Netlify, S3, etc.)

## Beat 2: The IRedirectable Interface

Introduce the `IRedirectable` capability interface and show which front matter types implement it.

### What to show
- Reference `T:Penn.FrontMatter.IRedirectable` defined in `:path src/Penn/FrontMatter/Capabilities.cs` -- a single-property interface: `P:Penn.FrontMatter.IRedirectable.RedirectUrl` (string?)
- Reference `T:Penn.DocSite.DocSiteFrontMatter` which implements `IRedirectable` alongside its other capabilities: `DocSiteFrontMatter : IFrontMatter, IDraftable, ITaggable, ISectionable, ICrossReferenceable, IOrderable, IDescribable, IRedirectable` -- see `:path src/Penn.DocSite/DocSiteFrontMatter.cs`
- Reference `T:Penn.BlogSite.BlogSiteFrontMatter` which also implements `IRedirectable`: `BlogSiteFrontMatter : IFrontMatter, IDraftable, ITaggable, IDescribable, IDateable, ICrossReferenceable, ISectionable, IRedirectable` -- see `:path src/Penn.BlogSite/BlogSiteFrontMatter.cs`
- Note that the core `T:Penn.FrontMatter.DocFrontMatter` does NOT implement `IRedirectable` -- only the site-level front matter types do
- When `RedirectUrl` is null (the default), no redirect behavior occurs; the page renders normally

### Key points
- `IRedirectable` follows the same capability pattern as `IDraftable`, `ITaggable`, etc. -- single-property interfaces checked via pattern matching
- The YAML key is `redirectUrl` (camelCase, matching `FrontMatterParser`'s `CamelCaseNamingConvention`)
- Both DocSite and BlogSite templates support redirects out of the box

## Beat 3: Create the Redirect File

Add a markdown file at the old URL path with `redirectUrl` pointing to the new location.

### What to show
- Create `Content/setup.md` with front matter:
  ```yaml
  title: "Setup (Moved)"
  redirectUrl: "/getting-started/"
  ```
- The body can contain a fallback message: `"This page has moved to [Getting Started](/getting-started/)."` for clients that do not follow meta-refresh
- The `T:Penn.FrontMatter.FrontMatterParser` deserializes the YAML via `M:Penn.FrontMatter.FrontMatterParser.Parse``1(System.String)`, populating `P:Penn.FrontMatter.IRedirectable.RedirectUrl` on the `DocSiteFrontMatter` record
- Reference `M:Penn.Routing.ContentRouteFactory.FromMarkdownFile(Penn.Routing.FilePath,Penn.Routing.FilePath,Penn.Routing.UrlPath,System.String)` which routes `setup.md` to `/setup/` with output file `setup/index.html`
- Also reference `M:Penn.Routing.ContentRouteFactory.ForRedirect(Penn.Routing.UrlPath)` which creates a `ContentRoute` specifically for redirect pages -- this is used in the pipeline's `T:Penn.Pipeline.RedirectSource` case of the `T:Penn.Pipeline.ContentSource` union

### Key points
- The redirect file is a real markdown file with front matter -- it goes through the normal discovery and parsing pipeline
- The `redirectUrl` value should be an absolute path (starting with `/`) for internal redirects
- External redirect URLs (starting with `https://`) also work

## Beat 4: How Redirects Flow Through the Pipeline

Trace how a redirect page is discovered, parsed, and ultimately generates redirect HTML during static site builds.

### What to show
- Discovery: `T:Penn.Content.MarkdownContentService`1` discovers `setup.md` like any other markdown file and yields a `T:Penn.Pipeline.DiscoveredItem` with a `T:Penn.Pipeline.MarkdownFileSource`
- Parsing: `T:Penn.Markdown.MarkdownContentParser`1` parses the file and produces a `T:Penn.Pipeline.ParsedItem` with the `DocSiteFrontMatter` metadata containing `RedirectUrl = "/getting-started/"`
- Pipeline-level filtering: `T:Penn.LlmsTxt.LlmsTxtService` skips redirect pages: `if (parsed.Metadata is IRedirectable { RedirectUrl: not null }) continue;` -- redirects are excluded from llms.txt generation
- SPA navigation: `T:Penn.Islands.SpaNavigationContentService` checks `item.Source is RazorPageSource or RedirectSource` to skip redirect sources from SPA island content
- Reference `T:Penn.Pipeline.ContentSource` union type which includes `T:Penn.Pipeline.RedirectSource` as one of its cases: `union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource)`
- `T:Penn.Pipeline.RedirectSource` holds `P:Penn.Pipeline.RedirectSource.TargetUrl` (a `T:Penn.Routing.UrlPath`)

### Key points
- Redirect pages are real content items that flow through the full pipeline, not a special bypass
- Other subsystems (LLMs.txt, SPA navigation) check for `IRedirectable` and skip redirect pages appropriately
- The `ContentSource` union ensures exhaustive handling of all source types including redirects

## Beat 5: Static Build -- Meta-Refresh HTML Generation

Run the build and inspect the generated redirect HTML file. Show the meta-refresh tag that performs the actual redirect.

### What to show
- Reference `T:Penn.Generation.OutputGenerationService` and its `M:Penn.Generation.OutputGenerationService.GenerateAsync(System.String)` method
- In `FetchPagesAsync`, when the HTTP response has status `MovedPermanently` or `Found`, the service generates redirect HTML:
  ```html
  <!DOCTYPE html>
  <html><head>
  <meta http-equiv="refresh" content="0;url=/getting-started/">
  <link rel="canonical" href="/getting-started/">
  </head></html>
  ```
- The redirect HTML is written to `setup/index.html` in the output directory
- The fetch result is recorded as `FetchOutcome.Redirect` and counted in `P:Penn.Generation.BuildReport.GeneratedPages` (redirects count as generated, not skipped)
- Reference the `FetchOutcome` enum: `Generated`, `Redirect`, `Failed`, `Error` -- redirects are a success outcome

### Key points
- The `<meta http-equiv="refresh" content="0;url=...">` tag causes an immediate client-side redirect
- The `<link rel="canonical">` tag tells search engines where the canonical content lives
- The `AllowAutoRedirect = false` setting on `HttpClient` in `OutputGenerationService` ensures the service captures the redirect response rather than following it

## Beat 6: Custom Front Matter Opt-In

Show how to add `IRedirectable` to a custom front matter type for projects that do not use `DocSiteFrontMatter` or `BlogSiteFrontMatter`.

### What to show
- A custom front matter record that does not implement `IRedirectable`:
  ```csharp
  public record MyFrontMatter : IFrontMatter, IDraftable { ... }
  ```
- Adding the interface:
  ```csharp
  public record MyFrontMatter : IFrontMatter, IDraftable, IRedirectable
  {
      public string Title { get; init; } = "";
      public bool IsDraft { get; init; }
      public string? RedirectUrl { get; init; }
  }
  ```
- Without `IRedirectable`, the `redirectUrl` YAML key is still parsed by `FrontMatterParser` (because it uses `IgnoreUnmatchedProperties`) but it has no effect -- no property on the record receives the value
- The pattern matching checks throughout the pipeline (`fm is IRedirectable { RedirectUrl: not null }`) will not match unless the interface is implemented

### Key points
- This is the same opt-in pattern as all other capabilities: implement the interface, add the property, and the behavior activates
- `FrontMatterParser` uses `IgnoreUnmatchedProperties()` so unknown YAML keys do not cause errors -- they are silently ignored
- No registration or configuration is needed beyond implementing the interface on the front matter record
