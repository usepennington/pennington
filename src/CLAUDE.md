# src/ — Library conventions

## C# style
- C# 15 union types: construct with `new UnionType(caseInstance)`; pattern-match on the case types directly.
- Records for data types, `ImmutableList<T>` for collections.
- File-scoped namespaces.
- LSP reports false errors on the `union` keyword and on some ASP.NET/Markdig types — the compiler handles them correctly, ignore the squiggles.

## XML docs
Every public type and member needs an xmldoc (CS1591 is on). Keep summaries concise and sufficient — a single sentence is usually enough.
- Records: `<summary>` on the type plus one `<param>` per positional parameter.
- Union case types get their own `<summary>`; the union itself gets a one-line `<summary>` describing the set.
- Net10.0 union shims under `#else` are hand-written: each `Value` property, constructor overload, and implicit operator needs its own one-line summary (e.g. `/// <summary>Wraps a <see cref="CaseType"/>.</summary>`). The net11.0 `union` branch synthesizes these, so only the shim needs them.
- Cross-reference with `<see cref="..."/>` rather than prose names.
- Don't restate the identifier — `<summary>Gets the title.</summary>` on a `Title` property adds nothing; describe the invariant or purpose instead.

## Front matter is capability-based
`IFrontMatter` (in `src/Pennington/FrontMatter/IFrontMatter.cs`) is the minimum contract — every page has a `Title`. Types opt into extra fields by implementing mixin interfaces in `src/Pennington/FrontMatter/Capabilities.cs`:

- `ITaggable` — tag collection for listings/filtering
- `ISectionable` — section grouping within a content area
- `IOrderable` — explicit ordering within a section
- `IRedirectable` — aliases / redirect sources

Compose these on a front matter record to declare what it parses. `DocFrontMatter` and `BlogSiteFrontMatter` are the canonical examples.

## Razor components
Components in `src/Pennington.UI/Components/` are single `.razor` files with inline `@code` — no separate code-behind, no separate CSS. Variants are parameter-driven via `switch` expressions on enums/strings. Match this shape for new components instead of introducing a partial-class or CSS-file pattern.

## DI lifetimes

Three tiers. Pick deliberately — the wrong lifetime silently caches stale data.

1. **Transient** (`AddTransient<T>`) — a new instance every resolve. The default choice. Use when the class has no state worth keeping between calls, or when it captures a file-watched service and is itself rebuilt often enough that direct ctor injection is safe.
2. **File-scoped** (`AddFileWatched<T>` in `Pennington.Infrastructure`) — a singleton-like instance that is ejected and rebuilt when `IFileWatcher` fires. Use for anything that holds state derived from content files (TOC metadata, xref lookup, search index, sitemap, `llms.txt`).
3. **Singleton** (`AddSingleton<T>`) — rare. Reserve for genuinely process-lifetime state: configuration records, the `IFileWatcher` itself, connection pools, registries that never change.

**Rule of thumb:** if in doubt between singleton and file-scoped, pick file-scoped. If in doubt between singleton and transient, pick transient.

### Services should inject file-watched deps directly — never reach for the factory

Classes that consume a file-watched service (e.g. `XrefResolver`, `NavigationBuilder`) take it by type in their ctor. They must not know about `FileWatchDependencyFactory<T>` or `IServiceProvider.GetRequiredService<T>()` — that's infrastructure plumbing. Keep domain code DI-idiomatic.

The knob you turn is **the consumer's own lifetime**, not the injection shape:

- **Stateless delegator / wrapper** → `AddTransient`. Every resolve rebuilds, pulling the current file-watched instance through. Examples: `XrefResolvingService`, `XrefHtmlRewriter`, `HtmlResponseRewritingProcessor`, `LlmsTxtContentService`.
- **Holds state that also depends on content files** → `AddFileWatched`. Rebuilds alongside its deps. Examples: `NavigationBuilder`, `LlmsTxtService`, `SearchIndexService`, `SitemapService`, `BlogSiteContentService`.
- **Must be singleton** for real reasons → then the direct capture *is* stale. In that case, push the file-watched concern down into a lower-lifetime collaborator rather than papering over it at the singleton's call sites. If you genuinely cannot avoid the singleton, raise it — it's a design smell worth discussing before introducing service-locator workarounds.

### The transient-chain property

Middleware and endpoint handlers resolve their dependencies per request via parameter injection (`InvokeAsync(HttpContext, IEnumerable<IResponseProcessor>)`). When a chain of transient services all depend on each other, each request rebuilds the whole chain. That's how the HTML-rewriting pipeline stays current without any consumer touching `FileWatchDependencyFactory` — the top-of-chain transient resolution cascades a fresh `XrefResolver` all the way through.

If you introduce a new link in such a chain, **keep it transient unless you have a specific reason not to**. A singleton in the middle pins the chain below it at its first resolution.

### Interface wrapper gotcha

`services.AddSingleton<IInterface>(sp => sp.GetRequiredService<ConcreteFileWatched>())` looks like a harmless forwarder but is a stale-reference factory — the outer `AddSingleton` caches the first `ConcreteFileWatched` it resolves and never asks again. Use `AddTransient<IInterface>(...)` so the inner `GetRequiredService` runs on every resolve and returns the factory's current instance. `BlogSite`'s `IContentService` wrapper and the core library's `LlmsTxtContentService` registration follow this pattern — copy it when registering a file-watched implementation behind an interface.

### Razor `@inject` on file-watched services

Blazor SSG / server-rendered pages resolve `@inject` from the per-request scope, so `@inject NavigationBuilder` picks up the current factory instance on each page render — no special handling needed for the built-in render path. If a component starts caching an injected file-watched service in a field between renders (e.g. persistent-state Blazor circuits), switch to per-call resolution at that site rather than spreading the pattern.
