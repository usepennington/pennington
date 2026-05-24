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

Middleware and endpoint wiring. Each method's xmldoc states its ordering invariant.

<ExtensionMethods Receiver="WebApplication" />

## Host runtime helpers

Entry points that dispatch between dev-serve and static-build based on `args[0]`. See [`RunOrBuildAsync`](xref:reference.api.pennington-extensions) for the dispatch contract.

## Example

A complete DocSite host wiring all three layers — `AddDocSite`, `UseDocSite`, `RunDocSiteAsync` — in call order.

```csharp:symbol
examples/DocSiteScaffoldExample/Program.cs
```

## `UseDocSite` middleware order

`UseDocSite` wraps a fixed middleware sequence before mapping the Razor component endpoint:

1. `UsePenningtonLocaleRouting`
2. `UseAntiforgery`
3. `UseStaticFiles`
4. `UseMonorailCss`
5. `UsePennington`
6. `MapRazorComponents<App>()`

`UseBlogSite` runs the same sequence without `UsePenningtonLocaleRouting`. For why each step lands where it does, see <xref:explanation.core.dev-vs-build>.

## See also

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [`PenningtonOptions`](xref:reference.api.pennington-options)
- Reference: [`DocSiteOptions`](xref:reference.api.doc-site-options)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
