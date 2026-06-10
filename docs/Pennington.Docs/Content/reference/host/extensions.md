---
title: "DI and middleware extension methods"
description: "Index of every AddPennington/UsePennington/Run* extension method across the referenced Pennington packages."
sectionLabel: "Host Integration"
order: 1
tags: [host, di, middleware, extensions]
uid: reference.host.extensions
---

The list of public extension methods Pennington exposes for wiring the library into an ASP.NET Core host — `Add*` (DI registration), `Use*` (middleware and endpoints), and `Run*` (host entry points). Grouped below by receiver type; each method is declared in an `*Extensions` static class under its owning feature namespace.

## `IServiceCollection` extensions

DI registration entry points. The three composition roots and the options record each configures:

- [`AddPennington`](xref:reference.api.pennington-extensions) — [`PenningtonOptions`](xref:reference.api.pennington-options)
- [`AddDocSite`](xref:reference.api.doc-site-service-extensions) — [`DocSiteOptions`](xref:reference.api.doc-site-options)
- [`AddBlogSite`](xref:reference.api.blog-site-service-extensions) — [`BlogSiteOptions`](xref:reference.api.blog-site-options)

The full set follows, each tagged with its owning package.

<ExtensionMethods Receiver="IServiceCollection" />

## `WebApplication` extensions

Middleware and endpoint wiring. The template `Use*` methods each wrap a fixed sequence, listed below.

<ExtensionMethods Receiver="WebApplication" />

### `UseDocSite` middleware order

`UseDocSite` registers this sequence before mapping the Razor component endpoint:

1. `UseLocaleRouting`
2. `UseAntiforgery`
3. `UseStaticFiles`
4. `UseMonorailCss`
5. `UsePennington`
6. `MapRazorComponents<App>()`

### `UseBlogSite` middleware order

`UseBlogSite` registers the same sequence minus locale routing, which BlogSite does not wire:

1. `UseAntiforgery`
2. `UseStaticFiles`
3. `UseMonorailCss`
4. `UsePennington`
5. `MapRazorComponents<App>()`

For why each step lands where it does, see <xref:explanation.core.dev-vs-build>.

## `Run*` host entry points

Host entry points that run one System.CommandLine pipeline: serve live with no verb, build the static site with `build`, or run a read-only inspection with `diag <sub>`. Build and diag run one-shot against a started in-memory host that is disposed afterward; serve hands off to `RunAsync`.

- [`RunOrBuildAsync`](xref:reference.api.pennington-extensions) — the core dispatcher; call it directly on a bare `AddPennington` host.
- [`RunDocSiteAsync`](xref:reference.api.doc-site-service-extensions) — DocSite wrapper over `RunOrBuildAsync`.
- [`RunBlogSiteAsync`](xref:reference.api.blog-site-service-extensions) — BlogSite wrapper over `RunOrBuildAsync`.

## Example

A complete DocSite host wiring all three layers — `AddDocSite`, `UseDocSite`, `RunDocSiteAsync` — in call order.

```csharp:symbol
examples/DocSiteScaffoldExample/Program.cs
```

## See also

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [`PenningtonOptions`](xref:reference.api.pennington-options)
- Reference: [`DocSiteOptions`](xref:reference.api.doc-site-options)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
