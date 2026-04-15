---
title: "Configure search indexing"
description: "Scope the client-side search index, tune priority, and exclude pages per-locale without replacing the search backend."
uid: how-to.configuration.search
order: 202020
sectionLabel: Configuration
tags: [search, front-matter, localization, configuration]
---

When your `/search-index-{locale}.json` endpoint is already live but results contain nav or footer noise, a page appears that should be hidden, or you need to adjust relative document weight, use the options described here to tune the index without touching the search client.

## Assumptions

- You have a working Pennington site and `/search-index-en.json` (or your default locale code) already returns a JSON array
- Your pages use `DocSiteFrontMatter` or another `IFrontMatter` implementation (which carries the `Search` default member)
- You know your default locale code (from `LocalizationOptions`) — it is the suffix in the index filename

The `DocSiteKitchenSinkExample` ships with the DocSite-pinned `#main-content` selector and a `Content/main/hidden.md` fixture demonstrating `search: false`.

---

## Steps

### 1. Exclude a markdown page with `search: false`

Add `search: false` to the page's front matter. The value flows through `IFrontMatter.Search` into `ContentTocItem.ExcludeFromSearch`; the index builder skips the page entirely while it continues to render at its URL and appear in the sidebar.

```yaml
---
title: Internal draft
search: false
---
```

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/hidden.md
```

### 2. Exclude a Razor `@page` with a metadata sidecar

Razor components do not carry YAML front matter, so `RazorPageContentService` loads a sibling `Foo.razor.metadata.yml` file. Place the sidecar next to the component — `search: false` there has the same effect as in a markdown page's front matter.

```yaml
title: Internal Tools
search: false
```

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.Search
```

### 3. Set the default document priority

`SearchIndexOptions.DefaultPriority` (default `5`) is the baseline weight assigned to every document whose content service does not override `IContentService.SearchPriority` — raise it for sources you want to outrank neighbours or lower it for auxiliary content. Per-source priority takes precedence: `MarkdownContentServiceOptions.SearchPriority` defaults to `10`, `RazorPageContentService` is `5`, and the llms.txt/SPA/redirect services report `0` so their artifacts never appear in results.

```csharp:xmldocid
P:Pennington.Search.SearchIndexOptions.DefaultPriority
```

Under `AddDocSite` this property is reachable via the `ConfigurePennington` escape hatch (`opts.SearchIndex.DefaultPriority = …`), so you do not need to drop down to bare `AddPennington` for this adjustment.

### 4. Override the content selector on DocSite

The selector scopes which HTML element's text becomes the search body. `DocSiteOptions.SearchIndexContentSelector` defaults to `#main-content` to match the stock `MainLayout.razor`; set it when you have replaced the layout or need to widen the indexed region to a different element. See <xref:explanation.core.docsite-positioning> for the cases that require dropping to bare `AddPennington`.

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.SearchIndexContentSelector
```

```csharp
services.AddDocSite(opts =>
{
    opts.SearchIndexContentSelector = "article.prose";
});
```

---

## Verify

- Run `dotnet run` and fetch `/search-index-{locale}.json`; the excluded page's `title` and `url` are absent from the `documents` array
- Add a second locale and observe one JSON file per locale (`/search-index-en.json`, `/search-index-fr.json`) — registered-but-empty locales return `[]` instead of 404
- Inspect one `documents[]` entry and confirm the `body` field contains only the scoped element's text (no header / sidebar / footer noise)

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Reference: [`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`](xref:reference.options.auxiliary-options)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
- How-to: [Generate an llms.txt](xref:how-to.configuration.llms-txt)
