---
title: "Adding Client-Side Search"
description: "Wire up client-side search: SearchIndexService builds /search-index.json, FlexSearch runs in the browser, configure per-source priority weighting, keyboard shortcuts, and lazy index loading"
uid: "penn.how-to.adding-client-side-search"
order: 20
---

## Beat 1: What DocSite Gives You for Free

Explain that DocSite wires search automatically — the reader only needs to configure priorities. If using Pennington core directly, the rest of this guide shows manual wiring.

### What to show
- DocSite's `MainLayout.razor` (:path `src/Pennington.DocSite/Components/Layout/MainLayout.razor`) includes a search input element with `id="search-input"` that triggers the search modal
- The `UsePennington` extension method (:path `src/Pennington/Infrastructure/PenningtonExtensions.cs`) registers the `/search-index.json` MapGet endpoint: `app.MapGet("/search-index.json", async (SearchIndexService service) => Results.Content(await service.GetSearchIndexJsonAsync(), "application/json"))`
- The Pennington.UI `scripts.js` (:path `src/Pennington.UI/wwwroot/scripts.js`) initializes `SearchManager` which binds to the `search-input` element, handles `Cmd/Ctrl+K`, creates the modal, and loads FlexSearch lazily
- If using DocSite, skip to Beat 5 for priority configuration

### Key points
- DocSite includes search out of the box: the trigger element, modal UI, keyboard shortcut, and search index endpoint are all pre-wired
- The only customization needed for DocSite users is configuring `SearchPriority` on content sources

## Beat 2: Register the Search Index Endpoint (Pennington Core)

For custom Pennington core sites, show that the `/search-index.json` endpoint is registered automatically by `UsePennington`, and explain how `SearchIndexService` builds the index.

### What to show
- Reference `T:Pennington.Search.SearchIndexService` (:path `src/Pennington/Search/SearchIndexService.cs`) — registered in DI by `AddPennington`, generates the search index lazily
- The constructor takes `IServiceProvider` and `T:Pennington.Search.SearchIndexBuilder` (:path `src/Pennington/Search/SearchIndexBuilder.cs`), wrapping the build in `AsyncLazy<string>` for thread-safe one-time computation
- Reference `M:Pennington.Search.SearchIndexService.GetSearchIndexJsonAsync` — returns the cached JSON string
- The endpoint is mapped in `UsePennington`: `app.MapGet("/search-index.json", ...)` (:path `src/Pennington/Infrastructure/PenningtonExtensions.cs`)
- The index is rebuilt on file changes when managed by `FileWatchDependencyFactory<T>` (mentioned in the XML doc comment on `SearchIndexService`)

### Key points
- The `/search-index.json` endpoint is automatic — `UsePennington` registers it for both DocSite and custom Pennington core sites
- The index is generated lazily on first request, not at startup
- During static build, the endpoint is fetched as a MapGet route in Phase 7 of `OutputGenerationService`

## Beat 3: How the Search Index Is Built

Walk through the index generation process: discovering content, parsing, rendering, and building search documents.

### What to show
- Reference the private `BuildSearchIndexAsync` method in `T:Pennington.Search.SearchIndexService` (:path `src/Pennington/Search/SearchIndexService.cs`)
- The method iterates all `T:Pennington.Content.IContentService` registrations via `sp.GetServices<IContentService>()`
- For each service, it calls `DiscoverAsync`, then `IContentParser.ParseAsync`, then `IContentRenderer.RenderAsync`
- Each successfully rendered item is passed to `M:Pennington.Search.SearchIndexBuilder.Build(Pennington.Pipeline.RenderedItem)` (:path `src/Pennington/Search/SearchIndexBuilder.cs`)
- Reference `T:Pennington.Search.SearchIndexDocument` (:path `src/Pennington/Search/SearchIndexDocument.cs`) — record with `Title`, `Body` (plain text, HTML stripped), `Url`, `Section`, `Locale`, `Priority`
- `SearchIndexBuilder.Build` skips drafts (`IDraftable { IsDraft: true }`), strips HTML from content using `StripHtml` (regex-based tag removal and entity decoding), and returns a `SearchIndexDocument`
- The final list is serialized with `System.Text.Json` using a source-generated `SearchIndexJsonContext` with camelCase naming

### Key points
- The search index contains plain text (HTML stripped), not raw HTML — this produces better search results
- Draft pages are excluded from the index
- The `Priority` field in each document comes from `SearchIndexBuilder._defaultPriority`, which is currently always 5 — per-source priority is controlled differently (see Beat 5)

## Beat 4: Add the Search Trigger Element (Custom Layout)

For custom Pennington core sites, show the minimal HTML needed to activate search.

### What to show
- Add a clickable element with `id="search-input"` to the layout — this is the element that `SearchManager` binds to in `scripts.js` (:path `src/Pennington.UI/wwwroot/scripts.js`)
- The `SearchManager.init()` method looks for `document.getElementById('search-input')` — if not found, it returns early and search is disabled
- When clicked, the element calls `openModal()` which creates and shows the search modal dynamically
- For subdirectory deployments, the `data-base-url` attribute on `<body>` (set by `T:Pennington.Infrastructure.BaseUrlRewritingProcessor`, :path `src/Pennington/Infrastructure/BaseUrlRewritingProcessor.cs`) is read by the search script to construct correct result URLs
- Reference the script inclusion: Pennington.UI's `scripts.js` is automatically available as a static web asset at `_content/Pennington.UI/scripts.js`

### Key points
- The trigger element must have `id="search-input"` — this is the convention `SearchManager` looks for
- No `data-search-trigger` attribute is needed; the binding is by element ID
- The `data-base-url` on `<body>` is set automatically by `BaseUrlRewritingProcessor` during build; for dev mode, it's only present when a non-root base URL is configured

## Beat 5: Configure Search Priority per Content Source

Show how `SearchPriority` on each content service controls result ranking.

### What to show
- Reference `P:Pennington.Content.IContentService.SearchPriority` (:path `src/Pennington/Content/IContentService.cs`) — `int` property on the interface, intended to determine ranking weight
- Each content service defines its own priority: `T:Pennington.Content.MarkdownContentService` defaults to `10` (via `P:Pennington.Content.MarkdownContentServiceOptions.SearchPriority`), `T:Pennington.Content.RazorPageContentService` has `SearchPriority = 5`, `T:Pennington.Islands.SpaNavigationContentService` has `SearchPriority = 0`, `T:Pennington.LlmsTxt.LlmsTxtContentService` has `SearchPriority = 0`
- **Note:** In the current implementation, `SearchPriority` is not yet wired into the search index. `T:Pennington.Search.SearchIndexBuilder` assigns a default priority of 5 to all documents regardless of the content service's `SearchPriority` value. The client-side `SearchManager` does read the `priority` field for scoring, but since all documents have the same priority, it has no differentiating effect. Also note that `MarkdownContentOptions` (the config-time type) has no `SearchPriority` property — the value lives on the runtime `MarkdownContentServiceOptions` but is not currently configurable from the `AddMarkdownContent` callback

### Key points
- The `SearchPriority` property exists on the interface but is not yet propagated to the search index — this is a known gap
- The client-side scoring infrastructure is ready (the `priority` field is used as a weight multiplier in `combineFieldResults`), but needs the server-side wiring to be effective
- All search results currently have equal priority weighting

## Beat 6: How FlexSearch Works Client-Side

Explain the client-side search architecture: lazy loading, index construction, and result ranking.

### What to show
- Reference `SearchManager` class in `scripts.js` (:path `src/Pennington.UI/wwwroot/scripts.js`)
- FlexSearch is loaded lazily via ES module import on first modal open: `await import('https://cdnjs.cloudflare.com/ajax/libs/FlexSearch/0.8.2/flexsearch.bundle.module.min.js')` — no upfront cost
- The search index JSON is fetched from `/search-index.json` (with `data-base-url` prepended if present): `let baseUrl = document.body.getAttribute('data-base-url') || ''`
- A FlexSearch `Document` index is created with fields `title` and `body`, using `tokenize: "forward"` and `Charset.LatinAdvanced` encoder
- Each document from the JSON is added to the index with its `url` (base-URL-adjusted), `title`, `body`, and `priority`
- Search results are combined across field results (`combineFieldResults`) and the `priority` value is used as a scoring weight

### Key points
- FlexSearch is a zero-dependency, pure JavaScript full-text search library
- The lazy loading pattern means zero impact on initial page load — FlexSearch module and search index are only fetched when the user first opens the search modal
- If the search index fetch fails, `searchIndexFailed` is set to true and subsequent opens show an error message (the fetch is not retried automatically)
- The `data-base-url` attribute ensures result URLs work correctly in subdirectory deployments

## Beat 7: The Keyboard Shortcut and Search Modal UX

Show the keyboard shortcut and modal behavior.

### What to show
- Reference the keydown listener in `SearchManager.setupEventListeners` (:path `src/Pennington.UI/wwwroot/scripts.js`): `(e.metaKey || e.ctrlKey) && e.key === 'k'` opens the modal; `Escape` closes it
- The `openModal` method removes the `hidden` class from the modal backdrop, focuses the input, and sets `document.body.style.overflow = 'hidden'` to prevent background scrolling
- The `createSearchModal` method dynamically constructs the modal DOM: a backdrop overlay, a search input with icon, and a results container
- Typing in the modal input triggers `performSearch` after a 300ms debounce
- Clicking the backdrop closes the modal

### Key points
- `Cmd+K` (Mac) and `Ctrl+K` (Windows/Linux) are the universal keyboard shortcuts — they match the convention used by GitHub, VS Code, and other developer tools
- The modal is created once in `init()` and toggled with CSS classes — no re-creation on each open
- The 300ms debounce prevents excessive search queries during rapid typing

## Beat 8: Verify the Search Index Contents

Show how to inspect the generated search index to confirm all content sources are included with correct metadata.

### What to show
- Navigate to `/search-index.json` in the browser during dev mode, or open the file from the build output directory
- The JSON is an array of `SearchIndexDocument` objects with camelCase property names: `title`, `body`, `url`, `section`, `locale`, `priority`
- Verify that documents from all registered content sources appear
- Check that draft pages are excluded
- Confirm that `body` contains plain text (no HTML tags) — `SearchIndexBuilder.StripHtml` (:path `src/Pennington/Search/SearchIndexBuilder.cs`) removes tags and decodes entities
- Confirm that `priority` values match the configured `SearchPriority` for each content source

### Key points
- The search index is a single JSON file containing all searchable content — there is no per-page index
- The `locale` field enables locale-aware search filtering in multi-locale sites
- The `section` field comes from the `ISectionable` interface on front matter, allowing section-based result grouping in the UI
