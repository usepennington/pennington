---
title: "Tune what the search box returns"
description: "Exclude pages from the index, weight document priority, and scope the indexed HTML region without replacing the search backend."
uid: how-to.configuration.search
order: 202020
sectionLabel: Configuration
tags: [search, front-matter, localization, configuration]
---

When `/search-index-{locale}.json` is already live but results contain nav or footer noise, a page appears that should be hidden, or relative document weight needs adjusting, the options below tune the index without touching the search client.

## Assumptions

- A working Pennington site where `/search-index-en.json` (or the default locale code) already returns a JSON array
- Pages using `DocSiteFrontMatter` or another `IFrontMatter` implementation (which carries the `Search` default member)
- The default locale code (from `LocalizationOptions`) — it is the suffix in the index filename

The `DocSiteKitchenSinkExample` ships with the DocSite-pinned `#main-content` selector and a `Content/main/hidden.md` fixture demonstrating `search: false`.

---

## Options

### Exclude a markdown page with `search: false`

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

### Exclude a Razor `@page` with a metadata sidecar

Razor components do not carry YAML front matter, so `RazorPageContentService` loads a sibling `Foo.razor.metadata.yml` file. Place the sidecar next to the component; `search: false` there has the same effect as in a markdown page's front matter.

```yaml
title: Internal Tools
search: false
```

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.Search
```

### Set the default document priority

`SearchIndexOptions.DefaultPriority` (default `5`) is the baseline weight assigned to every document whose content service does not override `IContentService.SearchPriority`. Raise it for sources that should outrank neighbours; lower it for auxiliary content. Per-source priority takes precedence: `MarkdownContentServiceOptions.SearchPriority` defaults to `10`, `RazorPageContentService` is `5`, and the llms.txt/SPA/redirect services report `0` so their artifacts never appear in results.

```csharp:xmldocid
P:Pennington.Search.SearchIndexOptions.DefaultPriority
```

Under `AddDocSite` this property is reachable via the `ConfigurePennington` escape hatch (`opts.SearchIndex.DefaultPriority = …`), so this adjustment does not require dropping down to bare `AddPennington`.

### Override the content selector on DocSite

The selector scopes which HTML element's text becomes the search body. `DocSiteOptions.SearchIndexContentSelector` defaults to `#main-content` to match the stock `MainLayout.razor`; set it after replacing the layout or to widen the indexed region to a different element. See <xref:explanation.core.docsite-positioning> for the cases that require dropping to bare `AddPennington`.

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

## Result

`/search-index-{locale}.json` returns one JSON object per indexed page. A typical entry, after the knobs above are applied:

```json
{
  "url": "/how-to/configuration/search/",
  "title": "Tune what the search box returns",
  "section": "Configuration",
  "body": "When /search-index-{locale}.json is already live but results contain nav or footer noise...",
  "priority": 10
}
```

Pages with `search: false` are absent from the array; per-source `SearchPriority` values populate the `priority` field.

## Verify

- Run `dotnet run` and fetch `/search-index-{locale}.json`. The excluded page's `title` and `url` are absent from the `documents` array
- Add a second locale and observe one JSON file per locale (`/search-index-en.json`, `/search-index-fr.json`). Registered-but-empty locales return `[]` instead of 404
- Inspect one `documents[]` entry and confirm the `body` field contains only the scoped element's text (no header / sidebar / footer noise)

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Reference: [`SearchIndexOptions`](xref:reference.api.search-index-options) — the knobs this how-to touches; see also [`HighlightingOptions`](xref:reference.api.highlighting-options), [`IslandsOptions`](xref:reference.api.islands-options), [`LlmsTxtOptions`](xref:reference.api.llms-txt-options), and [`OutputOptions`](xref:reference.api.output-options)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
- How-to: [Make the site discoverable to LLM crawlers](xref:how-to.configuration.llms-txt)
