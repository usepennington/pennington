---
title: "Define custom front-matter keys"
description: "Declare a record implementing IFrontMatter with extra YAML keys and register it through AddMarkdownContent so a markdown source deserializes into the custom type."
uid: how-to.pages.front-matter
order: 1
sectionLabel: "Pages"
tags: [front-matter, authoring, yaml]
---

To parse YAML keys the shipped front-matter records do not expose — a `namespace`, a `stability` badge, a `productName` — declare a custom `record` implementing `IFrontMatter` and the capability interfaces relevant to the keys, then register it as a markdown source with `AddMarkdownContent<T>`. For the full catalog of built-in keys, see <xref:reference.front-matter.keys>; for the design rationale behind the capability interfaces, see <xref:explanation.core.front-matter-capabilities>.

The recipe declares and registers the record in `examples/DocSiteKitchenSinkExample`, which adds `namespace` and `stability` keys on top of the built-in front-matter records, then reads those keys from a regular Razor page in `examples/CustomFrontMatterRazorPageExample`.

## Before you begin

- An existing Pennington site with markdown content under a `Content/` folder (see <xref:tutorials.getting-started.first-site> if not).
- A bare `AddPennington` host, or an existing `AddDocSite`/`AddBlogSite` host with room for an additional markdown source. `AddBlogSite` registers one source against `BlogSiteFrontMatter`; `AddDocSite` registers two sources (`DocSiteFrontMatter` and `BlogPostFrontMatter`). Adding a third custom-record source on top of the template is done by chaining another `AddMarkdownContent<T>()` call after `AddDocSite`/`AddBlogSite`, or by falling back to bare `AddPennington` (see <xref:how-to.discovery.multiple-sources>).

## Declare the record

Implement <xref:reference.api.i-front-matter> as a `record` and add only the capability interfaces the new keys need — `ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`. Unimplemented capabilities pick up their default-member values, so a minimal record is short.

```csharp:symbol
examples/DocSiteKitchenSinkExample/ApiFrontMatter.cs
```

Property names map to YAML keys under `CamelCaseNamingConvention` — `Namespace` reads `namespace:`; `Stability` reads `stability:`. Unknown keys are dropped with a warning in lenient mode (dev) and rejected in strict mode (the build default), so a typo on a custom key is flagged — as a dev warning or a build failure — rather than silently taking effect as a default.

## Register the record

Pass the record type to `AddMarkdownContent<T>` so the pipeline deserializes the YAML into that type. The configure delegate selects the content root the source reads from and the URL prefix its pages serve under. On a bare host this call goes inside the `AddPennington` lambda; on a DocSite or BlogSite host, chain it through the `ConfigurePennington` escape hatch so the extra source sits alongside the template's own. The kitchen-sink example registers its `ApiFrontMatter` source this way:

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.RegisterApiSource
```

`ExcludePaths` on the template's own doc source carves the subtree out so exactly one source owns those pages — drop that line on a bare `AddPennington` host where no template source claims the folder.

## Read the key in a Razor page

The lede promised a `stability` value — here is what consumes it. The `ApiFrontMatter` record is portable across hosts; only its registration differs, so this section uses a bare `AddPennington` host where a Razor page owns rendering. A page under the registered source authors the custom keys at the top of its YAML block:

```yaml
---
title: "Highlighting service"
namespace: "Pennington.Highlighting"
stability: "preview"
---
```

A regular Razor `@page` reads those keys by injecting `IPageResolver` and calling the generic `ResolveAsync<ApiFrontMatter>`. That overload resolves the requested URL and hands back the front matter already typed as the custom record, so `Stability` and `Namespace` are plain property reads — no cast. It returns `null` when nothing matches or the page is served by a source registered against a different front-matter type:

```razor:symbol
examples/CustomFrontMatterRazorPageExample/Components/Pages/SymbolPage.razor
```

Any page served by the `ApiFrontMatter` source now surfaces its typed keys — the round-trip from YAML to a strongly-typed Razor page.

## Verify

- In `examples/CustomFrontMatterRazorPageExample`, run `dotnet run` and visit `/symbols/highlighting-service/`. The page renders `Stability: preview` from that page's `stability:` key — proof the YAML deserialized into the typed `ApiFrontMatter.Stability` property and `ResolveAsync<ApiFrontMatter>` returned the typed page.
- The build report contains no `FrontMatterParseError` diagnostics for pages under the new source.

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys) — every built-in key, type, and default
- Reference: [Built-in front-matter types](xref:reference.api.doc-front-matter) — `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`
- Reference: [`IFrontMatter` and capability defaults](xref:reference.api.i-front-matter) — the capability interfaces available to a custom record
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities) — why the design collapsed ten interfaces into default members
- How-to: [Use multiple content sources](xref:how-to.discovery.multiple-sources) — chain a second `AddMarkdownContent<T>` against a custom record
