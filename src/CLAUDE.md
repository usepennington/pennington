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

1. **Transient** (`AddTransient<T>`) — a new instance every resolve. Use when the class has no state worth keeping between calls, or when it captures a file-watched service and needs to pick up the current instance each time.
2. **File-scoped** (`AddFileWatched<T>` in `Pennington.Infrastructure`) — a singleton-like instance that is ejected and rebuilt when `IFileWatcher` fires. Use for anything whose state depends on content files (TOC, xref map, search index, sitemap, llms.txt). Also use when the class itself has no state *but* captures another file-watched service in its ctor — the rebuild propagates.
3. **Singleton** (`AddSingleton<T>`) — rare. Reserve for genuinely process-lifetime state: configuration records, the `IFileWatcher` itself, connection pools, registries that never change.

### The stale-reference trap

A consumer that captures a file-watched service in its constructor pins the instance that was current at resolve time. When the factory ejects the file-watched service on a file change, the consumer still holds the old one.

- If the consumer is `AddFileWatched<>`, it gets rebuilt alongside — safe (`LlmsTxtService` capturing `NavigationBuilder`).
- If the consumer is `AddTransient<>` and nothing singleton captures the consumer in turn, every resolve gets a fresh wrapper that re-resolves the current file-watched instance — safe (`ContentResolver`).
- If the consumer is `AddSingleton<>`, the capture is stale forever. Fix by **injecting `FileWatchDependencyFactory<T>` (the DI singleton that owns the lifecycle) and reading `.Current` on each use**. This avoids the `IServiceProvider.GetRequiredService<T>()` service-locator pattern — the factory type is explicit in the ctor and the call site reads as a plain property. `XrefResolvingService` (`src/Pennington/Infrastructure/XrefResolvingService.cs`) and `LlmsTxtContentService` (`src/Pennington/LlmsTxt/LlmsTxtContentService.cs`) show the shape:

  ```csharp
  private readonly FileWatchDependencyFactory<TFileWatched> _depFactory;
  private TFileWatched Dep => _depFactory.Current;
  ```

### Interface wrapper gotcha

`services.AddSingleton<IInterface>(sp => sp.GetRequiredService<ConcreteFileWatched>())` looks like a harmless forwarder but is a stale-reference factory — the outer `AddSingleton` caches the first `ConcreteFileWatched` it resolves and never asks again. Use `AddTransient<IInterface>(...)` so the inner `GetRequiredService` runs on every resolve and returns the factory's current instance. The `BlogSite` and `Pennington` registrations follow this pattern — copy it when registering a file-watched implementation behind an interface.

### Razor `@inject` on file-watched services

Blazor SSG/server-rendered pages resolve `@inject` from the per-request scope, so `@inject NavigationBuilder` picks up the current factory instance on each page render — no special handling needed for the built-in render path. If a component starts caching an injected file-watched service in a field between renders (e.g., persistent-state Blazor circuits), switch that site to `@inject IServiceProvider` and resolve on each use.
