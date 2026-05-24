---
title: "Define custom front-matter keys"
description: "Declare a record implementing IFrontMatter with extra YAML keys and register it through AddMarkdownContent so a markdown source deserializes into the custom type."
uid: how-to.pages.front-matter
order: 201010
sectionLabel: "Pages"
tags: [front-matter, authoring, yaml]
---

To surface YAML keys the shipped front-matter records do not expose — an `apiVersion`, a `gitHubUrl`, a `productName` — declare a custom `record` implementing `IFrontMatter` and the capability interfaces relevant to the keys, then register it with `AddMarkdownContent<T>` on a bare `AddPennington` host. For the full catalog of built-in keys, see <xref:reference.front-matter.keys>; for the design rationale behind the capability interfaces, see <xref:explanation.core.front-matter-capabilities>.

The recipe references `examples/DocSiteKitchenSinkExample/ApiFrontMatter.cs`, which adds `apiVersion` and `gitHubUrl` keys on top of the built-in surface.

## Before you begin

- An existing Pennington site with markdown content under a `Content/` folder (see <xref:tutorials.getting-started.first-site> if not).
- A bare `AddPennington` host or an existing `AddDocSite`/`AddBlogSite` host with room for a second markdown source. `AddDocSite` and `AddBlogSite` each register one source against their own front-matter type; chaining a second record requires bare `AddPennington` (see <xref:how-to.discovery.multiple-sources>).

## Declare the record

Implement <xref:reference.api.i-front-matter> as a `record` and add only the capability interfaces the new keys need — `ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`. Unimplemented capabilities pick up their default-member values, so a minimal record is short.

```csharp:symbol
examples/DocSiteKitchenSinkExample/ApiFrontMatter.cs
```

Property names map to YAML keys under `CamelCaseNamingConvention` — `ApiVersion` reads `apiVersion:`; `GitHubUrl` reads `gitHubUrl:`. Unknown keys in the YAML are silently ignored, so a typo on a custom key surfaces as a default value rather than a parse error.

## Register the record

Pass the record type to `AddMarkdownContent<T>` so the pipeline deserializes the YAML into that type. The options delegate selects the content root the source reads from.

```csharp:symbol,bodyonly
src/Pennington/Infrastructure/PenningtonOptions.cs > PenningtonOptions.AddMarkdownContent
```

## Result

A page under the registered content source can now author the custom keys at the top of its YAML block:

```yaml
---
title: "Pennington API surface"
apiVersion: "1.0-alpha"
gitHubUrl: "https://github.com/usepennington/pennington"
---
```

Consumers read the typed properties on the resolved `IFrontMatter` via the content services that produced the page.

## Verify

- Run `dotnet run` and visit a page whose YAML uses the custom keys. The build report contains no `FrontMatterParseError` diagnostics for pages under the new source.
- Consume the typed property in a Razor component (`@inject IFrontMatter Fm` then cast to the custom record) and confirm the value round-trips from the YAML.

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys) — every built-in key, type, and default
- Reference: [Built-in front-matter types](xref:reference.api.doc-front-matter) — `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`
- Reference: [`IFrontMatter` and capability defaults](xref:reference.api.i-front-matter) — the capability interfaces available to a custom record
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities) — why the design collapsed ten interfaces into default members
- How-to: [Use multiple content sources](xref:how-to.discovery.multiple-sources) — chain a second `AddMarkdownContent<T>` against a custom record
