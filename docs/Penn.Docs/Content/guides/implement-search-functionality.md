---
title: "Implement Search Functionality"
description: "Add client-side search to your Penn site with SearchIndexBuilder and FlexSearch"
uid: "penn.guides.implement-search-functionality"
order: 2060
---

Penn builds a search index from your rendered content and serves it as JSON. A client-side FlexSearch engine loads that index and provides instant, fuzzy search with highlighting. You don't have to configure a search server, because there isn't one. The entire index ships as a static file. This is either a feature or a limitation, depending on how much content you have. For most documentation and blog sites it's plenty.

## How It Works

1. During the build, <xref:T:Penn.Search.SearchIndexBuilder> processes every <xref:T:Penn.Pipeline.RenderedItem> from the content pipeline.
2. Each item becomes a `SearchIndexDocument` with a title, body text (HTML stripped), URL, section, locale, and priority.
3. Drafts are excluded automatically -- `SearchIndexBuilder` checks for `IDraftable { IsDraft: true }` and returns `null`.
4. The resulting JSON is served at `/search-index.json` during dev and written to the output directory during build.
5. On the client, FlexSearch loads the JSON, indexes it, and powers the search modal.

### The SearchIndexDocument Shape

```csharp
public record SearchIndexDocument(
    string Title,
    string Body,       // Plain text, HTML tags stripped
    UrlPath Url,
    string? Section,
    string Locale,
    int Priority        // Higher = more prominent in results
);
```

The `Body` field comes from `SearchIndexBuilder.StripHtml()`, which removes tags, decodes common HTML entities, and collapses whitespace. It's not sophisticated -- it doesn't parse nested structures or handle edge cases in CDATA sections -- but it works well enough for search indexing.

## Prerequisites

- A Penn site with content (markdown or Razor pages)
- The Penn UI scripts included in your layout
- A search trigger element with `id="search-input"`

## Adding Search to Your Site

### 1. It's Already Registered

If you're using `AddPenn()`, the <xref:T:Penn.Search.SearchIndexBuilder> is registered automatically:

```csharp
// Inside AddPenn() -- you don't need to do this yourself
services.AddSingleton(_ => new SearchIndexBuilder());
```

If you're using `AddDocSite()`, search is fully wired up out of the box, including the UI. You can stop reading here and go make lunch.

### 2. Add the Search Trigger

In your layout, add an element with `id="search-input"`:

```html
<button type="button" id="search-input" class="w-full rounded-md">
    Search documentation...
</button>
```

The client-side scripts will:
- Attach a click handler that opens the search modal
- Register `Ctrl+K` / `Cmd+K` as a keyboard shortcut
- Fetch `/search-index.json` on first open (lazy-loaded, not eager)

### 3. Include the Scripts

In your `App.razor`, include the Penn UI script bundle and set the base URL for subdirectory deployments:

```html
<head>
    <script src="/_content/Penn.UI/scripts.js" defer></script>
</head>
```

For subdirectory deployments, the base URL is communicated via a `data-base-url` attribute on `<body>`, which the <xref:T:Penn.Infrastructure.BaseUrlRewritingProcessor> adds automatically.

### 4. Customize Search Priority (Optional)

Content services expose a `SearchPriority` property. Higher values push results toward the top:

```csharp
public class MyContentService : IContentService
{
    public int SearchPriority => 8; // Default is 5
    // ...
}
```

The `SearchIndexBuilder` uses a `defaultPriority` of 5, but individual content services can override this when building their `RenderedItem` records.

## Search Features

### Content Ranking

FlexSearch ranks results using field weights:

- **Title** (3x) -- what the page is called matters most
- **Description** (2x) -- the summary you wrote in front matter
- **Body** (1x) -- the full text, because sometimes people search for obscure things

The `Priority` field from `SearchIndexDocument` acts as a tiebreaker. Documentation pages (priority 10) outrank API reference pages (priority 5) when scores are otherwise equal.

### Content Processing

`SearchIndexBuilder.StripHtml()` is intentionally simple:

- Removes HTML tags via regex (`<[^>]+>`)
- Decodes `&amp;`, `&lt;`, `&gt;`, `&quot;`, `&#39;`, `&nbsp;`
- Collapses runs of whitespace to a single space

This means code blocks, navigation chrome, and other markup noise don't pollute your search index.

### Search UI

The built-in search modal provides:

- **Keyboard shortcut**: `Ctrl+K` (Windows/Linux) or `Cmd+K` (Mac)
- **Live results**: 300ms debounce, results appear as you type
- **Highlighting**: Search terms highlighted in titles and snippets
- **Graceful failure**: If the index can't load, you get a clear error message instead of a blank modal

### Styling

The search modal uses semantic CSS classes styled by MonorailCSS:

- `.search-modal-backdrop` -- the overlay behind the modal
- `.search-modal-content` -- the modal container
- `.search-result-item` -- each result row
- `.search-highlight` -- highlighted search terms

You can override these via MonorailCSS configuration or additional CSS.

## Troubleshooting

**Search modal doesn't open**: Confirm an element with `id="search-input"` exists in the DOM and that `scripts.js` loaded without errors.

**Empty results**: Check that `/search-index.json` returns data. If it's empty, verify your content services are producing `RenderedItem` records and that the search index builder is registered.

**Wrong URLs in results**: If you're deploying to a subdirectory, make sure the base URL is configured correctly. See [Deploying to Subdirectories](xref:penn.guides.deploying-to-subdirectories).

**Stale index during development**: The search index is rebuilt on each request in dev mode. If results seem stale, hard-refresh the page to clear the FlexSearch client cache.
