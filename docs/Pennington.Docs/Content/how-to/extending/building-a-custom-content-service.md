---
title: "Building a Custom Content Service"
description: "Implement IContentService to provide content from a non-markdown source (database, API, CMS) — covering the 5 required methods, 2 properties, DI registration, and file-watch integration"
uid: "penn.how-to.building-a-custom-content-service"
order: 10
---

## Beat 1: The Problem — Non-Markdown Content in a Markdown World

Introduce the scenario: Forge stores release notes as structured JSON files, not markdown. Pennington's default pipeline uses `T:Pennington.Content.MarkdownContentService`1` to discover `.md` files, but release notes live in `releases/*.json` with fields like `version`, `date`, `summary`, and `changes`. The goal is to feed these into Pennington's content pipeline so they appear as navigable HTML pages alongside the existing markdown docs.

### What to show
- A sample `releases/v2-1-0.json` file with structured data
- The gap: Pennington discovers markdown via `T:Pennington.Content.MarkdownContentService`1` but has no built-in JSON content discovery

### Key points
- Pennington's content pipeline is source-agnostic — any class implementing `T:Pennington.Content.IContentService` can feed content into it
- This pattern works for databases, REST APIs, CMSs, or any structured data source

## Beat 2: The IContentService Contract

Walk through the full `T:Pennington.Content.IContentService` interface. Explain each of the 5 methods and 2 properties and what role they play in the pipeline.

### What to show
- `M:Pennington.Content.IContentService.DiscoverAsync` — returns `IAsyncEnumerable<T:Pennington.Pipeline.DiscoveredItem>`, the entry point where content enters the pipeline
- `M:Pennington.Content.IContentService.GetContentToCopyAsync` — returns `ImmutableList<T:Pennington.Content.ContentToCopy>` for static files (images, downloads) to copy verbatim to output
- `M:Pennington.Content.IContentService.GetContentToCreateAsync` — returns `ImmutableList<T:Pennington.Content.ContentToCreate>` for dynamically generated files (JSON feeds, search indexes)
- `M:Pennington.Content.IContentService.GetContentTocEntriesAsync` — returns `ImmutableList<T:Pennington.Content.ContentTocItem>` for sidebar/navigation tree entries
- `M:Pennington.Content.IContentService.GetCrossReferencesAsync` — returns `ImmutableList<T:Pennington.Pipeline.CrossReference>` for `xref:` link resolution
- `P:Pennington.Content.IContentService.DefaultSection` — the navigation section name this service's content belongs to
- `P:Pennington.Content.IContentService.SearchPriority` — integer controlling search result ranking (higher = more prominent)

### Key points
- Every method has a distinct role in the pipeline: discovery, static assets, generated files, navigation, and cross-referencing
- All methods are async because content may come from I/O-bound sources
- `DiscoverAsync` uses `IAsyncEnumerable` for memory-efficient streaming of large content sets

## Beat 3: Define the Data Shape

Create a `ReleaseNote` record to represent the JSON structure. Create three sample JSON files in a `releases/` directory.

### What to show
- A `ReleaseNote` record with `Version` (string), `Date` (DateTime), `Summary` (string), `Changes` (string array)
- Three sample files: `releases/v2-1-0.json`, `releases/v2-0-1.json`, `releases/v2-0-0.json`

### Key points
- The data shape is application-specific — Pennington does not prescribe it
- Keep the record simple; the content service handles the transformation to Pennington's types

## Beat 4: Scaffold the Content Service Class

Create `ReleaseNotesContentService` implementing `T:Pennington.Content.IContentService`. Set the two properties and stub out all five methods.

### What to show
- Class declaration: `public sealed class ReleaseNotesContentService : IContentService`
- `P:Pennington.Content.IContentService.DefaultSection` set to `"Releases"` — this controls which navigation section the content appears under
- `P:Pennington.Content.IContentService.SearchPriority` set to `50` — higher than the default markdown priority of `10` so release notes rank above general docs in search
- Constructor accepting a path to the `releases/` directory
- All five methods stubbed with `throw new NotImplementedException()`

### Key points
- `DefaultSection` maps directly to the sidebar section heading in `T:Pennington.Navigation.NavigationBuilder`
- `SearchPriority` is relative — Pennington's `T:Pennington.Content.MarkdownContentService`1` uses a configurable value, typically around `10`

## Beat 5: Implement DiscoverAsync — Entering the Pipeline

Implement `M:Pennington.Content.IContentService.DiscoverAsync` to read JSON files and yield `T:Pennington.Pipeline.DiscoveredItem` records with `T:Pennington.Pipeline.ProgrammaticSource` content sources.

### What to show
- Iterate over `releases/*.json` files, deserialize each to `ReleaseNote`
- For each release, create a `T:Pennington.Routing.ContentRoute` using `M:Pennington.Routing.ContentRouteFactory.FromCustom(Pennington.Routing.UrlPath,Pennington.Routing.FilePath,System.String)` with canonical path `/releases/v{version}/` — this factory method is designed for non-markdown content services. Note that `sourceFile` and `locale` are optional (defaulting to `null` and `""` respectively)
- Show the `T:Pennington.Routing.ContentRoute` record structure: `P:Pennington.Routing.ContentRoute.CanonicalPath` (`T:Pennington.Routing.UrlPath`), `P:Pennington.Routing.ContentRoute.OutputFile` (`T:Pennington.Routing.FilePath`), and optional `P:Pennington.Routing.ContentRoute.SourceFile`
- Create a `T:Pennington.Pipeline.ProgrammaticSource` wrapping an `T:Pennington.Pipeline.IProgrammaticContentGenerator` implementation
- The generator's `M:Pennington.Pipeline.IProgrammaticContentGenerator.GenerateAsync(Pennington.Routing.ContentRoute)` method returns a `T:Pennington.Pipeline.TextProgrammaticContent` with rendered HTML and null metadata (the HTML is the final output)
- Yield `new DiscoveredItem(route, new ContentSource(new ProgrammaticSource(generator)))` — note the `T:Pennington.Pipeline.ContentSource` union wrapping

### Key points
- `T:Pennington.Pipeline.ContentSource` is a union type with four cases: `T:Pennington.Pipeline.MarkdownFileSource`, `T:Pennington.Pipeline.RazorPageSource`, `T:Pennington.Pipeline.RedirectSource`, `T:Pennington.Pipeline.ProgrammaticSource` — custom content services use `ProgrammaticSource`
- `T:Pennington.Pipeline.IProgrammaticContentGenerator` has one method: `M:Pennington.Pipeline.IProgrammaticContentGenerator.GenerateAsync(Pennington.Routing.ContentRoute)` returning `T:Pennington.Pipeline.ProgrammaticContent`
- `T:Pennington.Pipeline.ProgrammaticContent` is itself a union: `T:Pennington.Pipeline.TextProgrammaticContent` (for HTML/text) or `T:Pennington.Pipeline.BinaryProgrammaticContent` (for images, PDFs, etc.)
- `T:Pennington.Pipeline.TextProgrammaticContent` accepts optional `T:Pennington.FrontMatter.IFrontMatter` metadata, raw content string, and content type (defaults to `"text/html"`)

## Beat 6: Implement GetContentTocEntriesAsync — Navigation Integration

Return `T:Pennington.Content.ContentTocItem` records so each release appears in the sidebar navigation.

### What to show
- For each release, create a `T:Pennington.Content.ContentTocItem` with:
  - `Title`: the version string (e.g., "v2.1.0")
  - `Route`: the same `T:Pennington.Routing.ContentRoute` from discovery
  - `Order`: derived from version number (e.g., `210` for v2.1.0) so releases sort correctly
  - `HierarchyParts`: `["releases", "v2-1-0"]` — this `string[]` determines the nesting depth in the sidebar tree
  - `Section`: the `DefaultSection` value (`"Releases"`)
  - `Locale`: `null` for single-locale sites
- Show the `T:Pennington.Content.ContentTocItem` record signature: `ContentTocItem(string Title, ContentRoute Route, int Order, string[] HierarchyParts, string? Section, string? Locale)`

### Key points
- `HierarchyParts` is the key to sidebar nesting — `["releases"]` would create a flat list, `["releases", "v2-1-0"]` creates a parent "releases" node with child entries
- `Order` controls sort position within a hierarchy level — lower values appear first
- The `Section` field groups content in the sidebar; it must match `P:Pennington.Content.IContentService.DefaultSection` for consistency

## Beat 7: Implement GetCrossReferencesAsync — xref Support

Return `T:Pennington.Pipeline.CrossReference` records so other pages can link to releases with `xref:forge.release.v2-1`.

### What to show
- For each release, create a `T:Pennington.Pipeline.CrossReference` with:
  - `Uid`: `"forge.release.v{version}"` (e.g., `"forge.release.v2-1-0"`)
  - `Title`: the version display name
  - `Route`: the content route
- Show the record signature: `CrossReference(string Uid, string Title, ContentRoute Route)`
- Demonstrate usage in a markdown file: `See [release notes](xref:forge.release.v2-1-0)` renders as a link to `/releases/v2-1-0/`

### Key points
- Cross-references are resolved by `T:Pennington.Infrastructure.XrefResolvingProcessor` during response processing — the processor finds `xref:` links in rendered HTML and replaces them with actual URLs
- UIDs must be globally unique across all content services
- Cross-references enable loose coupling between content pages — authors reference by UID, not by URL path

## Beat 8: Implement the Remaining Methods

Fill in `GetContentToCopyAsync` (empty — no static assets) and `GetContentToCreateAsync` (one generated JSON feed file).

### What to show
- `M:Pennington.Content.IContentService.GetContentToCopyAsync` returns `ImmutableList<T:Pennington.Content.ContentToCopy>.Empty` — release notes have no associated static files to copy. Show the `T:Pennington.Content.ContentToCopy` record signature: `ContentToCopy(FilePath SourcePath, FilePath OutputPath)` for reference
- `M:Pennington.Content.IContentService.GetContentToCreateAsync` returns a single `T:Pennington.Content.ContentToCreate` for `/releases/feed.json`:
  - `OutputPath`: `new FilePath("releases/feed.json")`
  - `ContentGenerator`: a `Func<Task<byte[]>>` that serializes all releases to a JSON array
  - `ContentType`: `"application/json"`
- Show the record signature: `ContentToCreate(FilePath OutputPath, Func<Task<byte[]>> ContentGenerator, string ContentType)`

### Key points
- `T:Pennington.Content.ContentToCreate` is for files that do not correspond to navigable pages — they are generated during static build but do not appear in navigation or search
- The `ContentGenerator` func is lazy — it runs only during static build, not during discovery
- Common uses: RSS feeds, JSON APIs, search indexes, aggregated data exports

## Beat 9: Register and Run

Wire the content service into DI and verify everything works end-to-end.

### What to show
- Registration in `Program.cs`: `services.AddSingleton<IContentService, ReleaseNotesContentService>()`
- Explain that Pennington discovers all `T:Pennington.Content.IContentService` registrations — there is no limit on the number of content services. The existing `T:Pennington.Content.MarkdownContentService`1` for markdown and the new `ReleaseNotesContentService` for JSON both feed the same pipeline
- Run the site and verify:
  - Navigate to `/releases/v2-1-0/` — the rendered HTML page appears
  - Check the sidebar — a "Releases" section with three version entries
  - Test xref linking from a markdown page
  - Hit `/releases/feed.json` — the aggregated JSON feed endpoint

### Key points
- Multiple `IContentService` implementations coexist naturally — the pipeline merges content from all registered services
- Registration order does not matter; the pipeline processes all services

## Beat 10: Add File Watching for Live Reload

Integrate with `T:Pennington.Infrastructure.FileWatchDependencyFactory`1` so changes to JSON files trigger a live reload.

### What to show
- Instead of plain `AddSingleton`, use `M:Pennington.Infrastructure.FileWatchedServiceExtensions.AddFileWatched``2(Microsoft.Extensions.DependencyInjection.IServiceCollection)` to register the service: `services.AddFileWatched<IContentService, ReleaseNotesContentService>()`
- Explain how `T:Pennington.Infrastructure.FileWatchDependencyFactory`1` works: it holds a cached instance, and when `T:Pennington.Infrastructure.IFileWatcher` fires (any watched file changes), `M:Pennington.Infrastructure.FileWatchDependencyFactory`1.InvalidateInstance` disposes the old instance and creates a fresh one on next access
- In the constructor, call `M:Pennington.Infrastructure.IFileWatcher.AddPathWatch(System.String,System.String,System.Action{System.String,System.IO.WatcherChangeTypes},System.Boolean)` to watch the `releases/` directory for `*.json` changes
- Demonstrate: add a `v2-2-0.json` file while the dev server is running — the page appears automatically

### Key points
- `T:Pennington.Infrastructure.FileWatchDependencyFactory`1` is the standard pattern for file-watching in Pennington — services like `XrefResolver`, `SearchIndexService`, and `SitemapService` use the same mechanism via `:path:src/Pennington/Infrastructure/FileWatchedServiceExtensions.cs`. Note: `T:Pennington.Content.MarkdownContentService`1` uses a different approach — it registers via `Activator.CreateInstance` in `AddPennington` and handles file watching internally in its constructor
- The factory creates the service via `ActivatorUtilities.CreateInstance<T>` so constructor injection still works
- `M:Pennington.Infrastructure.IFileWatcher.SubscribeToChanges(System.Action)` registers a callback that fires when watched files change. `FileWatchDependencyFactory` subscribes to invalidate its cached instance; `T:Pennington.Infrastructure.LiveReloadServer` subscribes independently to push browser reloads
