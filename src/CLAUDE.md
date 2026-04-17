# src/ — Library conventions

## C# style
- C# 15 union types: construct with `new UnionType(caseInstance)`; pattern-match on the case types directly.
- Records for data types, `ImmutableList<T>` for collections.
- File-scoped namespaces.
- LSP reports false errors on the `union` keyword and on some ASP.NET/Markdig types — the compiler handles them correctly, ignore the squiggles.

## Front matter is capability-based
`IFrontMatter` (in `src/Pennington/FrontMatter/IFrontMatter.cs`) is the minimum contract — every page has a `Title`. Types opt into extra fields by implementing mixin interfaces in `src/Pennington/FrontMatter/Capabilities.cs`:

- `ITaggable` — tag collection for listings/filtering
- `ISectionable` — section grouping within a content area
- `IOrderable` — explicit ordering within a section
- `IRedirectable` — aliases / redirect sources

Compose these on a front matter record to declare what it parses. `DocFrontMatter` and `BlogSiteFrontMatter` are the canonical examples.

## Razor components
Components in `src/Pennington.UI/Components/` are single `.razor` files with inline `@code` — no separate code-behind, no separate CSS. Variants are parameter-driven via `switch` expressions on enums/strings. Match this shape for new components instead of introducing a partial-class or CSS-file pattern.
