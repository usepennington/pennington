---
title: Configure search indexing
description: Tune the FlexSearch index — content selector, default priority, per-page opt-outs, and per-locale output files.
section: configuration
order: 40
tags: []
uid: how-to.configuration.search
isDraft: true
search: false
llms: false
---

> **In this page.** Tuning `SearchIndexOptions.ContentSelector`, setting `DefaultPriority`, opting pages out via `search: false` or Razor-page sidecar metadata, and per-locale output files.
>
> **Not in this page.** Replacing the FlexSearch client or building a server-side search backend.

## When to use this

- Your site's full-body search is too noisy (headers, footers, sidebars pollute results) and you want to scope the index to the main article.
- You have a large content source whose pages should still render but never appear in search.
- You want to understand why the generator writes `search-index-{locale}.json` per locale and how pages land in each bucket.

## Assumptions

- You have a working Pennington site (doc, blog, or custom) with at least one `IContentService` registered.
- You already reach the built-in client at `/search-index-{locale}.json` through `UsePennington()`.
- You know which front matter type each content source uses (`DocFrontMatter`, `BlogFrontMatter`, or your own `IFrontMatter`).

To copy a working setup, see [`examples/SearchExample`](https://github.com/Pennington/Pennington/tree/main/examples/SearchExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Narrow the indexed body with `ContentSelector`

Set a CSS selector so `RenderedHtmlFetcher` returns only the main article element. When null the entire `<body>` is indexed, which is almost never what you want.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.SearchIndex.ContentSelector = "article";
});
```

### 2. Set `DefaultPriority` for tie-breaking

`SearchIndexBuilder` stamps every emitted `SearchIndexDocument` with `SearchIndexOptions.DefaultPriority` (default `5`). Raise or lower it to bias FlexSearch ranking between sites that share a client.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.SearchIndex.ContentSelector = "article";
    penn.SearchIndex.DefaultPriority = 10;
});
```

### 3. Opt a markdown page out with `search: false`

`MarkdownContentService` reads `IFrontMatter.Search` and sets `ContentTocItem.ExcludeFromSearch`. `SearchIndexService` skips those entries while still rendering them for navigation.

```yaml
title: Internal runbook
search: false
```

### 4. Opt a Razor `@page` out with sidecar metadata

`RazorPageContentService` loads `<ComponentName>.razor.metadata.yml` from the same directory as the component and honors `search: false` the same way.

```yaml
title: Debug dashboard
search: false
```

### 5. Emit per-service exclusions from custom content services

If you build `ContentTocItem`s directly, set `ExcludeFromSearch` from your metadata's `Search` flag — otherwise per-page opt-outs in your custom source are silently ignored.

```csharp:xmldocid
T:SearchExample.Services.RandomContentService
```

### 6. Verify the per-locale output files

One file per configured locale is mapped by `UsePennington()`; registered-but-empty locales still serve `[]`. Unconfigured locales 404.

```
/search-index-en.json
/search-index-es.json
```

---

## Verify

- Run `dotnet run` and fetch `/search-index-{locale}.json` for each configured locale; expect a JSON array.
- Confirm entries you marked `search: false` are absent and entries in other locales do not leak into the default-locale file.
- Inspect one document's `body` field to confirm it contains only the text inside your `ContentSelector`.

## Related

- Reference: [`SearchIndexOptions`](/reference/search/search-index-options)
- Reference: [`IContentService`](/reference/content/icontentservice)
- Background: [Search index architecture](/explanation/search-index)
