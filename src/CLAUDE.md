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

Three tiers:

- **Transient** (`AddTransient<T>`) — default. Stateless services, and anything that captures a file-watched dep.
- **File-scoped** (`AddFileWatched<T>` in `Pennington.Infrastructure`) — holds state derived from content files. Ejected and rebuilt when `IFileWatcher` fires. Examples: `NavigationBuilder`, `XrefResolver`, `SearchArtifactService`, `BlogSiteContentService`.
- **Singleton** (`AddSingleton<T>`) — rare. Process-lifetime state only: options records, `IFileWatcher`, connection pools.

**Ctors take deps by type.** No `IServiceProvider`, `FileWatchDependencyFactory<T>`, or `Func<T>` in domain ctors — that's plumbing. If a direct capture would go stale, fix the consumer's lifetime, not the injection shape. A singleton capturing a file-watched dep is a smell; push the dep down to a transient collaborator.

**Interface wrapper gotcha.** `AddSingleton<IFoo>(sp => sp.GetRequiredService<FileWatchedFoo>())` caches the first concrete and never refreshes. Use `AddTransient<IFoo>` — the inner resolve runs per call.
