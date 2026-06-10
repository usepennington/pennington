---
title: "Tune what the search box returns"
description: "Exclude pages from the index, weight document priority, and scope the indexed HTML region without replacing the search backend."
uid: how-to.discovery.search
order: 2
sectionLabel: "Content Discovery"
tags: [search, front-matter, localization, configuration]
---

When the search index is already live but results contain nav or footer noise, a page appears that should be hidden, or relative document weight needs adjusting, the options below tune the index without touching the search client. For how the index is built, sharded, and queried, see <xref:explanation.discovery.search>.

## Before you begin
- A working Pennington site that serves `/search/en/index.json` (or the default locale code) — the search index entrypoint
- Pages using `DocSiteFrontMatter` or another `IFrontMatter` implementation (which carries the `Search` default member)
- The default locale code (from `LocalizationOptions`) — it is the suffix in the index filename

[`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) ships with the DocSite-pinned `#main-content` selector and a `Content/main/hidden.md` fixture demonstrating `search: false`.

---

## Options

### Exclude a markdown page with `search: false`

Add `search: false` to the page's front matter. The index builder skips the page entirely while it continues to render at its URL and appear in the sidebar.

```yaml
---
title: Internal draft
search: false
---
```

```markdown:symbol
examples/DocSiteKitchenSinkExample/Content/main/hidden.md
```

### Exclude a Razor `@page` with a metadata sidecar

Razor components do not carry YAML front matter, so `RazorPageContentService` loads a sibling `Foo.razor.metadata.yml` file. Place the sidecar next to the component; `search: false` there has the same effect as in a markdown page's front matter.

```yaml
title: Internal Tools
search: false
```

### Set the default document priority

`SearchIndexOptions.DefaultPriority` (default `5`) is the baseline weight assigned to every document whose content service does not override `IContentService.SearchPriority`. Raise it for sources that should outrank neighbors; lower it for auxiliary content. Per-source overrides take precedence — see <xref:reference.api.search-index-options> for the shipped defaults.

Under `AddDocSite` this property is reachable via the `ConfigurePennington` escape hatch (`ConfigurePennington = penn => penn.SearchIndex.DefaultPriority = …`), so this adjustment does not require dropping down to bare `AddPennington`.

### Override the content selector on DocSite

The selector scopes which HTML element's text becomes the search body — and the same element drives llms.txt sidecars and the build-time link audit, so chrome is stripped once. `DocSiteOptions.ContentSelector` defaults to `#main-content` to match the stock `MainLayout.razor`; set it after replacing the layout or to widen the indexed region. See <xref:explanation.positioning.docsite-positioning> for the cases that require dropping to bare `AddPennington`.

```csharp
services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    SiteDescription = "Project documentation",
    ContentSelector = "article.prose",
});
```

### Add query synonyms

To make a term also match alternates, set `SearchIndexOptions.Synonyms`. Keys and values are stemmed at build time and shipped in the entrypoint, so authors write natural words; the client expands query terms as it searches.

```csharp
services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    SiteDescription = "Project documentation",
    ConfigurePennington = penn =>
        penn.SearchIndex.Synonyms = new Dictionary<string, string[]>
        {
            ["config"] = ["configuration", "settings"],
        },
});
```

### Choose which facets the client can filter by

`SearchIndexOptions.Facets` selects the dimensions surfaced as filter chips: content area (the first URL segment after any locale prefix), section, and tags. Only area is on by default — it stays a short, stable list that reads well as chips. Section and tag vocabularies grow large enough to bury the filter bar, so opt into them when the extra filtering is worth the chips.

```csharp
services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    SiteDescription = "Project documentation",
    ConfigurePennington = penn =>
        penn.SearchIndex.Facets = SearchFacetField.Area | SearchFacetField.Section | SearchFacetField.Tags,
});
```

---

## Result

The build emits the index under `/search/{locale}/`: an `index.json` entrypoint with the document table, facet labels, and ranking stats, plus `t-*.json` term shards and `f-*.json` per-page fragments. A document-table row in the entrypoint, after the knobs above are applied:

```json
{
  "u": "/how-to/configuration/search/",
  "t": "Tune what the search box returns",
  "l": 142,
  "p": 10,
  "f": { "section": [0], "tag": [2, 5], "area": [1] }
}
```

The page body lives in its fragment (`f-{docId}.json`), fetched only when the page appears in results. Pages with `search: false` are absent from the table; per-source `SearchPriority` values populate `p`.

## Verify

- Run `dotnet run` and fetch `/search/{locale}/index.json`. The excluded page is absent from the `docs` table
- Add a second locale and observe one index tree per locale (`/search/en/index.json`, `/search/fr/index.json`). Registered-but-empty locales return a valid entrypoint with an empty `docs` array
- Fetch the matching `/search/{locale}/f-{docId}.json` and confirm its `body` contains only the scoped element's text (no header / sidebar / footer noise)
- After raising `DefaultPriority` (or a per-source `SearchPriority`), fetch `index.json` and confirm the affected rows carry the new value in their `p` field
- After adding a synonym, fetch `index.json` and confirm the stemmed synonym map carries the entry; in the modal, query the key term and confirm pages that mention only the alternate now appear
- After enabling the section and tag facets, fetch `index.json` and confirm rows carry `section` and `tag` ids in their `f` object; in the modal, confirm the matching filter chips appear above the results

## Related

- How-to: [Add the search modal to a non-DocSite site](xref:how-to.discovery.search-on-a-bare-host) — surface this index in a search UI on a bare host
- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Reference: [`SearchIndexOptions`](xref:reference.api.search-index-options) — the knobs this how-to touches; see also [`HighlightingOptions`](xref:reference.api.highlighting-options), [`LlmsTxtOptions`](xref:reference.api.llms-txt-options), and [`OutputOptions`](xref:reference.api.output-options)
- Background: [How the search index is built and queried](xref:explanation.discovery.search)
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning)
- How-to: [Make the site discoverable to LLM crawlers](xref:how-to.feeds.llms-txt)
