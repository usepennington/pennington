# FusionCacheDocSiteExample

A real-world DocSite whose API reference is generated from a single NuGet package (`ZiggyCreatures.FusionCache`) via the reflection-based metadata backend — no live compilation, no staged DLL/XML, no vendored source.

## Concepts

- `AddApiMetadataFromCompiledAssembly(opts => opts.FromPackageReference(...))`
- `AddApiReference` auto-publishing `/api/{slug}/` pages
- `<ApiSummary>`, `<ApiMemberTable>`, `<ApiParameterTable>` Mdazor components on the resulting pages. The `*Table` suffix is the Razor component name, not the rendered HTML element — `<ApiMemberTable>` emits a `<dl>`-based definition list (with per-member `<article>` blocks for methods/constructors/events) rather than an HTML `<table>`. Style it via the surrounding `dl`/`dt`/`dd`/`article` elements, not `table`/`tr`/`td`.
- Mixing hand-written guides with generated API reference in one sidebar

## Type-name slugs

`AddApiReference` slugs a type name by inserting `-` before each uppercase letter whose neighbour is lowercase. Acronym runs stay glued together:

- `FusionCache` → `fusion-cache`
- `FusionCacheBuilderExtMethods` → `fusion-cache-builder-ext-methods`
- `FusionCacheXMLOptions` → `fusion-cache-xml-options` (the `XML` run is preserved — *not* `x-m-l`)
- `IOStream` → `io-stream`
- `HTTPSConfig` → `https-config`

Regression-tested in `tests/Pennington.IntegrationTests/DocsSite/ApiReferenceIndexSlugTests.cs`.

## Referenced from

- `docs/.../how-to/content-services/auto-api-reference.md` — the "Wire the reflection backend" section uses this example as the canonical NuGet-backed target.
