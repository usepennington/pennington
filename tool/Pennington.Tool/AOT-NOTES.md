# Why this tool is not Native-AOT (yet)

The original ask was a **.NET AOT console** dotnet tool. This PoC ships as a normal
framework-dependent global tool instead. The blocker is not the tool's own code — it is the
rendering stack the two site templates are built on. This note records exactly what stands in
the way so the AOT question can be revisited deliberately.

## The core problem: Blazor SSR is reflection-driven

Both templates render their pages through Blazor static server-side rendering
(`app.MapRazorComponents<App>()`). As of .NET 11 there is **no source-generated rendering path**
for Razor components — component activation, parameter binding, and the renderer's component
factory all go through reflection. Native AOT requires that all such metadata be statically
discoverable at publish time; the Razor component runtime does not provide it. `PublishAot` on a
project that calls `MapRazorComponents` produces trim/AOT analyzer warnings and fails at runtime
once a code path the trimmer removed is hit.

## Concrete blockers in this repo

1. **`DynamicComponent` with a runtime `Type`.** The catch-all page router in both templates
   (`Pennington.DocSite/Components/Layout/Pages.razor`,
   `Pennington.BlogSite/Components/Pages/Pages.razor`) renders the resolved page via
   `<DynamicComponent Type="..." />`, where the type is found by **scanning assemblies at
   runtime**. This is reflection-based component dispatch — the canonical AOT/trim hazard.

2. **Reflection-based `@page` discovery.** Routable pages are discovered by reflecting over
   `Assembly.GetEntryAssembly()` and any `AdditionalRoutingAssemblies`. AOT cannot see routes
   that only exist as runtime-reflected attributes.

3. **`Microsoft.AspNetCore.TestHost` is a product dependency of core.** Build/diag modes swap
   Kestrel for `TestServer` to crawl the site in-process. `TestHost` is not authored for trim/AOT.

4. **SharpYaml reflection fallback.** Front-matter parsing prefers a source-generated YAML
   context but falls back to reflection-based (de)serialization when one is absent. The fallback
   is AOT-hostile.

5. **Broad reflection-prone dependencies.** AngleSharp (DOM), TextMateSharp (grammar loading),
   and Mdazor (component round-tripping) are all product dependencies of core and none declare
   trim/AOT safety.

There is also no existing groundwork: a repo-wide search for `PublishAot`, `PublishTrimmed`,
`IsTrimmable`, or `EnableTrimAnalyzer` finds nothing. This would be greenfield.

## What a path to AOT would actually require

Not a tool change — an upstream change to the templates and core:

- A **source-generated / statically-rooted page router** to replace the `DynamicComponent` +
  assembly-scanning dispatch (or wait for first-class AOT support for Razor component SSR).
- Moving `TestHost` out of the product graph (e.g. a real in-memory loopback crawl, or a
  build-only dependency).
- Guaranteeing the **source-generated YAML context** is always used; removing the reflection
  fallback under AOT.
- A trim/AOT pass over AngleSharp / TextMateSharp / Mdazor usage, with feature switches and
  `TrimmerRootDescriptor`s where needed.

A pragmatic interim step short of full AOT is **self-contained + single-file + trimmed**
(`PublishSingleFile` / `PublishTrimmed`), which gives a standalone binary without requiring the
reflection-free guarantees AOT demands — at the cost of triaging trim warnings.

## What this PoC does instead

Ships as a framework-dependent global tool (`PackAsTool`) targeting .NET 11. It runs the real
serve / build / diag pipeline against both templates, configured entirely from `pennington.toml`.
That delivers the user-visible goal (a folder + a TOML → a served or built site) today, and
isolates the AOT question to the upstream rendering stack where it actually lives.
