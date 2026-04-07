---
title: "Implement Search Functionality"
description: "Add client-side search to your Penn site with SearchIndexBuilder and FlexSearch"
uid: "penn.guides.implement-search-functionality"
order: 2060
---

Penn builds a search index from your rendered content and serves it as a static JSON file. On the client, FlexSearch loads that file and provides instant, fuzzy search with term highlighting. There is no search server. The entire index ships as `/search-index.json`, which makes it work identically in dev mode and after static site generation.

## How Search Works End-to-End

The pipeline has a server side and a client side.

**Server side:**

1. `SearchIndexService` iterates over every registered `IContentService`, discovering and rendering all content through the standard parse/render pipeline.
2. For each `RenderedItem`, `SearchIndexBuilder` strips HTML from the rendered output, extracts metadata, and produces a `SearchIndexDocument`.
3. Drafts are excluded automatically -- the builder checks for `IDraftable { IsDraft: true }` and returns `null`.
4. The list of documents is serialized to JSON using a source-generated `JsonSerializerContext` with camelCase property naming.
5. Penn maps the endpoint `GET /search-index.json` in `UsePenn()`, which returns the serialized index with `application/json` content type.

**Client side:**

6. When the user first opens the search modal, the `SearchManager` class (in `scripts.js`) dynamically imports FlexSearch 0.8.2 from a CDN.
7. It fetches `/search-index.json` (adjusting for any base URL on subdirectory deployments).
8. FlexSearch indexes the documents using forward tokenization with Latin-advanced encoding.
9. As the user types, a 300ms debounced search runs against the local index. Results are scored, ranked, and displayed with highlighted terms.

The index is computed lazily. `SearchIndexService` wraps the build in an `AsyncLazy<string>`, so the JSON is only generated on first request. During development, the service is managed by `FileWatchDependencyFactory`, which recreates the instance when content files change on disk. You get fresh search results without restarting the server.

## The SearchIndexDocument Shape

Each document in the index has six fields:

```csharp
public record SearchIndexDocument(
    string Title,       // From IFrontMatter.Title
    string Body,        // Plain text -- HTML tags stripped
    string Url,         // Canonical URL path
    string? Section,    // From ISectionable.Section, if implemented
    string Locale,      // From ContentRoute.Locale
    int Priority        // Ranking weight (default 5)
);
```

The `Body` field is produced by `SearchIndexBuilder.StripHtml()`. This method removes HTML tags with a regex, decodes the six most common HTML entities (`&amp;`, `&lt;`, `&gt;`, `&quot;`, `&#39;`, `&nbsp;`), and collapses whitespace. Code blocks, navigation chrome, and other markup are reduced to plain text so they don't pollute search results.

The `Section` field is populated only when the content's front matter implements `ISectionable`. For most sites this maps to the content source's configured section name.

The `Url` field holds the canonical path (e.g., `/guides/implement-search-functionality`). During subdirectory deployments, the client-side code prepends the base URL when building result links.

## What DocSite Wires Up Automatically

If you are using `AddDocSite()`, search works out of the box. No configuration needed. Here is what happens behind the scenes:

- `AddDocSite()` calls `AddPenn()`, which registers `SearchIndexBuilder` as a singleton and `SearchIndexService` via `AddFileWatched<SearchIndexService>()`.
- `UsePenn()` maps the `/search-index.json` endpoint.
- The DocSite layout includes a `<button id="search-input">` in the header, which the `SearchManager` uses as its activation trigger.
- The `App.razor` shell includes `<script src="/_content/Penn.UI/scripts.js" defer>`, which contains the `SearchManager` class.

If you are building with [DocSite](xref:penn.guides.using-docsite), you can stop reading here. Everything below applies to custom sites that use `AddPenn()` directly.

## Adding Search to a Custom Site

When you call `AddPenn()`, the search index infrastructure is registered automatically. You need to provide three things in your layout: a trigger element, the script reference, and (optionally) a base URL attribute for subdirectory deployments.

### 1. Registration Is Automatic

`AddPenn()` handles all service registration:

```csharp
// These lines are inside AddPenn() -- you don't write them yourself
services.AddSingleton(_ => new SearchIndexBuilder());
services.AddFileWatched<SearchIndexService>();
```

And `UsePenn()` maps the endpoint:

```csharp
// Also inside UsePenn()
app.MapGet("/search-index.json", async (SearchIndexService service) =>
    Results.Content(await service.GetSearchIndexJsonAsync(), "application/json"));
```

You get this for free by calling `AddPenn()` and `UsePenn()`.

### 2. Add the Search Trigger Element

The `SearchManager` looks for an element with `id="search-input"`. If it doesn't find one, search is silently disabled. Add a button to your layout:

```html
<button type="button" id="search-input">
    <svg viewBox="0 0 20 20" fill="none" aria-hidden="true">
        <circle cx="11" cy="11" r="8" stroke="currentColor"></circle>
        <path d="M21 21l-4.35-4.35" stroke="currentColor"></path>
    </svg>
    Search
    <kbd>Ctrl K</kbd>
</button>
```

Style this however you want. The only requirement is `id="search-input"`.

### 3. Include the Script

In your `App.razor` or equivalent HTML shell, reference the Penn.UI script bundle:

```html
<head>
    <script src="/_content/Penn.UI/scripts.js" defer></script>
</head>
```

This script contains the `PageManager` class, which initializes a `SearchManager` instance on page load. The `SearchManager` creates the search modal dynamically, attaches event listeners, and handles the full search lifecycle.

## Client-Side Search Behavior

### Lazy Loading

FlexSearch is not bundled with `scripts.js`. It is loaded as an ES module from a CDN the first time the user opens the search modal:

```javascript
const flexSearchModule = await import(
    'https://cdnjs.cloudflare.com/ajax/libs/FlexSearch/0.8.2/flexsearch.bundle.module.min.js'
);
this.FlexSearch = flexSearchModule.default;
```

The search index JSON is also fetched lazily at this point. Until the user activates search, no network requests are made for search-related resources.

If either the FlexSearch import or the index fetch fails, the manager sets a `searchIndexFailed` flag and stops retrying. Subsequent modal opens display a "Search is currently unavailable" message.

### Indexing and Scoring

FlexSearch is configured with a Document index that stores `title`, `body`, `url`, and `section`, and indexes on `title` and `body`:

```javascript
this.searchIndex = new this.FlexSearch.Document({
    tokenize: "forward",
    encoder: this.FlexSearch.Charset.LatinAdvanced,
    cache: 100,
    document: {
        id: 'id',
        store: ["title", "body", "url", "section"],
        index: ["title", "body"]
    }
});
```

Forward tokenization means a search for "mark" matches "markdown" but not "remark". The Latin-advanced encoder handles accented characters and common transliterations.

When a search runs, FlexSearch returns separate result arrays per indexed field. The `combineFieldResults` method merges them with weighted scoring:

| Field | Weight |
|-------|--------|
| `title` | 3x |
| `body` | 1x |

Within each field, documents appearing earlier in the result array receive a higher position score (`1 / (index + 1)`). The `priority` value from the search index document acts as a multiplier on top of that. A document with priority 10 scores twice as high as one with priority 5, all else being equal.

### Debounce

Search input is debounced at 300ms. Each keystroke resets the timer. Only when the user pauses typing does the search execute. This prevents unnecessary computation while the user is still forming their query.

## Search UI Features

### Keyboard Shortcut

Press `Ctrl+K` (Windows/Linux) or `Cmd+K` (macOS) to open the search modal from anywhere on the page. Press `Escape` to close it.

### The Modal

The `SearchManager` builds the modal DOM dynamically on initialization. It consists of:

- A backdrop overlay (`.search-modal-backdrop`) that closes the modal on click
- A search input with a magnifying glass icon
- A results container that shows one of four states: placeholder text, a loading indicator, search results, or an error message

When the modal opens, it sets `document.body.style.overflow = 'hidden'` to prevent background scrolling. Closing the modal restores scrolling and clears the input.

### Result Display

Each result shows:

- The page title with matching terms wrapped in `<mark class="search-highlight">` tags
- A content snippet (up to 150 characters) centered on the first occurrence of a matching term, also with highlighting

Results link directly to the page. Clicking a result closes the modal. In SPA-enabled sites, the navigation happens without a full page reload.

### Snippet Generation

The `getContentSnippet` method locates the first matching term in the body text and extracts 75 characters on each side. Terms shorter than 3 characters are ignored to avoid highlighting noise like "a" or "to". If no term is found, the snippet falls back to the first 150 characters of the body.

## Customizing Search Priority

The `SearchIndexBuilder` constructor accepts a `defaultPriority` parameter (default: 5). `AddPenn()` registers it with the default value:

```csharp
services.AddSingleton(_ => new SearchIndexBuilder());
```

To change the default priority for all documents, register the builder yourself before calling `AddPenn()`, or replace the registration after:

```csharp
services.AddSingleton(_ => new SearchIndexBuilder(defaultPriority: 10));
```

Each `IContentService` also exposes a `SearchPriority` property. `MarkdownContentService` defaults to 10; `RazorPageContentService` defaults to 5. You can set this per source through `MarkdownContentServiceOptions`:

```csharp
penn.AddMarkdownContent<MyFrontMatter>(md =>
{
    md.ContentPath = "Content/guides";
    md.BasePageUrl = "/guides";
    md.SearchPriority = 15; // Boost guides above other content
});
```

The priority value appears in the JSON index and is used by the client-side scoring algorithm as a multiplier on the position-weighted field score.

## Subdirectory Deployments

When deploying to a subdirectory (e.g., `/my-project/`), URLs in the search index and the path to `search-index.json` itself need adjustment. Penn handles this through the `data-base-url` attribute on `<body>`.

The `BaseUrlRewritingProcessor` adds this attribute automatically during static builds when a base URL is provided:

```bash
dotnet run -- build /my-project
```

The `SearchManager` reads the attribute to construct the correct index URL and to prefix result links:

```javascript
let baseUrl = document.body.getAttribute('data-base-url') || '';
const searchIndexUrl = baseUrl ? `${baseUrl}/search-index.json` : '/search-index.json';
```

Result URLs are also prefixed with the base URL when rendering links in the modal. This means search results point to the correct paths regardless of where the site is hosted.

For more details on subdirectory configuration, see [Deploying to Subdirectories](xref:penn.guides.deploying-to-subdirectories).

## Troubleshooting

**Search modal does not open.** Confirm that an element with `id="search-input"` exists in the DOM. Open the browser console and check that `scripts.js` loaded without errors. The `SearchManager.init()` method returns early if it cannot find the trigger element.

**"Search is currently unavailable" message.** The FlexSearch CDN import or the `/search-index.json` fetch failed. Check the browser console for network errors. If you are behind a corporate proxy that blocks CDN resources, you may need to self-host the FlexSearch module.

**Empty results for content that exists.** Verify that `/search-index.json` returns data by requesting it directly in the browser. If the array is empty, check that your content services are producing `RenderedItem` records and that the items are not marked as drafts. Also confirm that `SearchIndexBuilder` and `SearchIndexService` are registered (they are if you called `AddPenn()`).

**Wrong URLs in search results.** For subdirectory deployments, verify that `<body data-base-url="/your-path">` is present in the rendered HTML. If it is missing, the `BaseUrlRewritingProcessor` may not be running -- check that you passed the base URL argument to the build command.

**Stale results during development.** `SearchIndexService` is managed by `FileWatchDependencyFactory`, which recreates the service instance when content files change on disk. If results still seem stale, the browser may have cached the old `search-index.json`. Hard-refresh the page or clear the browser cache.

**Search works in dev but not after build.** Confirm that `/search-index.json` is present in your output directory. The static build discovers it through the mapped endpoint. If the file is missing, check the build report output for errors.
