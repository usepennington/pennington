# MultipleSourcesExample

Bare `AddPennington` host with two `AddMarkdownContent<T>` calls pointing at different content roots and different front-matter types (`DocFrontMatter` for docs, `BlogFrontMatter` for blog posts). One `MapGet` resolves either by walking every `IContentService`.

## Concepts

- Multiple markdown sources in one host
- Per-source `ContentPath` / `BasePageUrl` / `ExcludePaths`
- Per-source front-matter types
- Overlap demonstration toggled by the `MULTIPLE_SOURCES_OVERLAP=1` env var

## Referenced from

- `docs/.../how-to/discovery/multiple-sources.md`
