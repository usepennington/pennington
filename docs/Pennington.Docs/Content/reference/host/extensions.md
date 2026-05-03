---
title: "DI and middleware extension methods"
description: "Index of every AddPennington/UsePennington/Run* extension method across the Pennington, DocSite, BlogSite, MonorailCSS, and Roslyn packages."
sectionLabel: "Host Integration"
order: 406010
tags: [host, di, middleware, extensions]
uid: reference.host.extensions
---

The complete roster of public extension methods Pennington exposes for wiring the library into an ASP.NET Core host — `Add*` (DI registration), `Use*` (middleware and endpoints), and `Run*` (host entry points). Grouped below by receiver type; each method is declared in an `*Extensions` static class under its owning feature namespace.

## `IServiceCollection` extensions

DI registration entry points. Each method's options surface is documented on its own reference page, linked from the method's xmldoc.

<ExtensionMethods Receiver="IServiceCollection" />

## `WebApplication` extensions

Middleware and endpoint wiring. Ordering within a `Use*` call chain is load-bearing; see each method's xmldoc for the invariant.

<ExtensionMethods Receiver="WebApplication" />

## Host runtime helpers

Entry points that dispatch between dev-serve and static-build based on `args[0]`. Dev and build share one rendering pipeline; the `build` branch calls `app.StartAsync()`, resolves `OutputGenerationService`, crawls the running host via HTTP, and writes to `OutputOptions.OutputDirectory`.

See [`RunOrBuildAsync`](xref:reference.api.pennington-extensions) on the `PenningtonExtensions` type page for the full dispatch contract.

## Example

A complete DocSite host wiring all three layers — `AddDocSite`, `UseDocSite`, `RunDocSiteAsync` — in their canonical call order.

```csharp:path
examples/DocSiteScaffoldExample/Program.cs
```

The same three-call shape holds for every template: `Add*` builds the service graph, `Use*` mounts the middleware and endpoints, `Run*Async` reads `args` and either serves or builds.

## See also

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [`PenningtonOptions`](xref:reference.api.pennington-options)
- Reference: [`DocSiteOptions`](xref:reference.api.doc-site-options)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
