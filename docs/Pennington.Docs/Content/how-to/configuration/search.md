---
title: "Configure search indexing"
description: "Scope the client-side search index, tune priority, and exclude pages per-locale without replacing the search backend."
uid: how-to.configuration.search
order: 20
sectionLabel: Configuration
tags: [search, front-matter, localization, configuration]
---

> **In this page.** _Paraphrase TOC "Covers": tuning `SearchIndexOptions.ContentSelector`, setting `DefaultPriority`, opting pages out with `search: false` (markdown) or a Razor-page sidecar, and the per-locale `/search-index-{code}.json` output. Two sentences max._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": swapping the FlexSearch client or standing up a server-side backend is out of scope — link to the client-side search explanation or the search reference page for the index shape._

## When to use this

_Two sentences. Frame the realistic arrival state: the reader already has a Pennington site whose `/search-index-{code}.json` endpoint is live, search results are either including noise (chrome markup, nav text) or missing a page that should not appear, and they want to tune what lands in the index. Do not re-teach how search is wired at the host level — link back to the DocSite scaffold tutorial for setup._

## Assumptions

_Keep to 3 bullets. Must establish that search is already running so the knobs below have somewhere to land._

- You have a working Pennington site and `/search-index-en.json` (or your default locale code) already returns a JSON array
- Your pages use `DocSiteFrontMatter` or another `IFrontMatter` implementation (which carries the `Search` default member)
- You know your default locale code (from `LocalizationOptions`) — it is the suffix in the index filename

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/hidden.md` is the canonical `search: false` fixture and the site ships with the DocSite-pinned `#main-content` selector. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four steps. One for DocSite-idiomatic exclusion (`search: false`), one for Razor-page sidecar exclusion, one for `DefaultPriority` tuning, one for custom `ContentSelector` on bare `AddPennington`. Verb-first headings, prose under two sentences per step. Do not exceed 7 steps total._

### 1. Exclude a markdown page with `search: false`

_One sentence: `search: false` in front matter flows through `IFrontMatter.Search` into `ContentTocItem.ExcludeFromSearch`, which the index builder skips — the page still renders at its URL and still appears in the sidebar. This is the DocSite-idiomatic knob since `AddDocSite` pins `ContentSelector = "#main-content"` to match the stock layout._

```yaml
---
title: Internal draft
search: false
---
```

_Point the reader at the fixture:_

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/hidden.md
```

### 2. Exclude a Razor `@page` with a metadata sidecar

_One sentence: Razor components do not carry YAML front matter, so `RazorPageContentService` loads a sibling `Foo.razor.metadata.yml` file; `search: false` there has the same effect as on a markdown page. Place the sidecar next to the component file so the discovery pass picks it up._

```yaml
title: Internal Tools
search: false
```

_Show the front-matter property that `Search: false` is binding against:_

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.Search
```

### 3. Set the default document priority

_Two sentences. `SearchIndexOptions.DefaultPriority` (default `5`) is the baseline weight assigned to every document whose content service does not override `IContentService.SearchPriority` — raise it for sources you want to outrank neighbours, lower it for auxiliary content. Per-source priority still wins: `MarkdownContentServiceOptions.SearchPriority` defaults to `10`, `RazorPageContentService` is `5`, and the llms.txt/SPA/redirect services report `0` so their artifacts never rank._

```csharp:xmldocid
P:Pennington.Search.SearchIndexOptions.DefaultPriority
```

_Note: under `AddDocSite` this property is reachable through the `ConfigurePennington` escape hatch — `opts.SearchIndex.DefaultPriority = …` — so DocSite users do not have to drop down to bare `AddPennington` for this knob._

### 4. Override the content selector (bare `AddPennington` only)

_Two sentences. The selector scopes which HTML element's text becomes the search body — pick an id or tag that excludes chrome, nav, and footer. Under `AddDocSite` this is pinned to `#main-content` to match the stock `MainLayout.razor`; consumers who need a different selector should drop to bare `AddPennington` (see the positioning explanation linked below)._

```csharp:xmldocid
P:Pennington.Search.SearchIndexOptions.ContentSelector
```

_Show the assignment shape (plain C# fence — no symbol body to quote):_

```csharp
services.AddPennington(opts =>
{
    opts.SearchIndex.ContentSelector = "article.prose";
});
```

---

## Verify

_Terse. Three bullets — one per knob the reader just turned._

- Run `dotnet run` and fetch `/search-index-{locale}.json`; the excluded page's `title` and `url` are absent from the `documents` array
- Add a second locale and observe one JSON file per locale (`/search-index-en.json`, `/search-index-fr.json`) — registered-but-empty locales return `[]` instead of 404
- Inspect one `documents[]` entry and confirm the `body` field contains only the scoped element's text (no header / sidebar / footer noise)

## Related

_Two to four cross-quadrant links: the reference page for the index record shape, the positioning explanation for the bare-`AddPennington` drop-down path, and the sibling how-to on `llms.txt` since `search:`/`llms:` are symmetric front-matter flags. Do not link to the next how-to in this section — auto-generated._

- Reference: [Front matter key reference](/reference/front-matter/keys)
- Reference: [`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`](/reference/options/auxiliary-options)
- Background: [When is DocSite the right starting point?](/explanation/core/docsite-positioning)
- How-to: [Generate an llms.txt](/how-to/configuration/llms-txt)
