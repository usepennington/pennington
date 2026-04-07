---
title: "JavaScript Architecture"
description: "The modular client-side JavaScript system that powers Penn's interactive features"
uid: "penn.under-the-hood.javascript-architecture"
order: 3050
---

Penn generates static HTML. Every page works without JavaScript -- search engines index it, browsers render it, screen readers read it. But a documentation site with zero interactivity is spartan. So Penn ships `scripts.js`, a modular client-side system that adds theme switching, search, code tabs, scroll-tracking page outlines, and other quality-of-life features.

The architecture follows a single principle: each feature is a self-contained manager class, and `PageManager` coordinates them all. Nothing initializes unless the DOM elements it needs are present. Nothing loads external libraries until they are actually needed.

## Architecture Overview

```
PageManager (entry point, window.pageManager)
  |
  +-- ThemeManager         -- dark/light mode, localStorage persistence
  +-- OutlineManager       -- scroll-tracking table of contents
  +-- TabManager           -- ARIA-compliant tabbed code blocks
  +-- SyntaxHighlighter    -- client-side highlight.js fallback
  +-- MermaidManager       -- lazy CDN load, OKLCH-to-hex, theme-aware re-render
  +-- MobileNavManager     -- sidebar hamburger toggle
  +-- MainSiteNavManager   -- top-level site nav hamburger
  +-- SidebarToggleManager -- ToC sidebar panel toggle
  +-- SearchManager        -- Ctrl+K, lazy FlexSearch, weighted scoring
```

All managers are instantiated once on page load. After SPA navigation, `PageManager` selectively reinitializes the managers that operate on page content (outline, tabs, syntax highlighting, mermaid). Managers that bind to persistent chrome (theme, nav, search) keep their original state.

## PageManager

`PageManager` is the entry point. It detects DOM ready state, instantiates every manager, calls their `init()` methods, and exposes itself as `window.pageManager` for console debugging.

Two event listeners wire it into the SPA lifecycle:

- **`spa:before-navigate`** -- calls `onSpaNavigating()`, which clears the outline so the old headings do not linger during fetch.
- **`spa:commit`** -- calls `onSpaCommit(url)`, which reinitializes content-dependent managers and updates the active navigation link by matching `data-current` attributes against the new URL pathname.

This is the glue between the [SPA Island Architecture](xref:penn.under-the-hood.spa-island-architecture) and the component managers. The SPA engine handles page fetching and DOM injection; `PageManager` handles re-initialization of interactive features inside the injected content.

## ThemeManager

Manages dark/light theme switching with `localStorage` persistence. Binds to any element carrying the `data-theme-toggle` attribute. When toggled, it:

1. Flips the `dark` class on `document.documentElement`.
2. Sets `dataset.theme` to `"light"` or `"dark"`.
3. Writes the preference to `localStorage.theme`.
4. Triggers `MermaidManager.reinitializeForTheme()` so diagrams re-render with the new color scheme.

Theme preference is read before first paint by an inline `<script>` in the `<head>`, avoiding the white flash when the user prefers dark mode. For details on how themes integrate with Penn's CSS design tokens, see [Configure Custom Styling](xref:penn.guides.configure-custom-styling).

## OutlineManager

Provides a scroll-tracking table of contents in the sidebar. As you scroll, the currently visible heading is highlighted in the outline navigation.

Scroll detection uses a `requestAnimationFrame`-throttled handler with a `{ passive: true }` listener. The `isScrolling` flag ensures only one rAF callback is queued at a time. The handler walks the sections array in reverse, using `getBoundingClientRect()` to find the last heading at or above a reading position offset (130px header + 50px). If no heading qualifies, the first one is selected.

Outline generation works in two modes. If the server rendered outline markup, `OutlineManager` discovers the existing links inside `[data-role="page-outline"] ul`. If the container has a `data-content-selector` attribute, the manager dynamically extracts H2 and H3 headings from the content, builds a parent/child structure (H3s nest under the preceding H2), and renders the list.

A visual highlighter element (`[data-role="page-outline-highlighter"]`) tracks the active link with absolute positioning and CSS opacity transitions. On SPA navigation, `reset()` aborts the previous scroll listener via `AbortController` and clears the section map, preventing stale DOM references.

## TabManager

Handles tabbed content blocks -- typically switching between language variants in code examples. Each tab group is a `[role="tablist"]` element with an `id`.

Full ARIA semantics: `role="tab"` on buttons, `aria-controls` pointing to panel IDs, `tabindex` management (`0` for active, `-1` for inactive), and `data-state` toggling. Panels are shown/hidden via the `hidden` attribute and `data-selected`.

Panel visibility uses an ID convention: button IDs follow `tabButton{GroupName}-{Index}`, panel IDs follow `tab-content{GroupName}-{Index}`. The `hideRelatedContentPanels` method parses the group name from the button ID to hide all panels in the group before showing the selected one.

## MermaidManager

Renders Mermaid diagrams with theme-aware styling. Diagrams are authored as `mermaid` fenced code blocks and detected via `code.language-mermaid` elements.

**Lazy CDN loading.** If no mermaid code blocks exist on the page, the library is never fetched. On first encounter, a dynamic `import()` from `cdn.jsdelivr.net/npm/mermaid@11` loads and caches the module.

**OKLCH-to-hex conversion.** Penn's CSS uses OKLCH color variables (via [MonorailCSS](xref:penn.guides.configure-custom-styling)), but Mermaid only understands hex. `getMermaidConfig()` reads computed CSS custom properties and converts OKLCH values through the OKLab color space pipeline: OKLCH to OKLab, to linear LMS, to linear RGB, to sRGB, to hex. Fallback hex values cover conversion failures.

**Theme-aware re-rendering.** When `ThemeManager` toggles, it calls `reinitializeForTheme()`, which reconfigures Mermaid with new color variables and re-renders every tracked diagram. Timestamp-based unique IDs (`mermaid-diagram-{timestamp}-{index}`) force Mermaid to apply the new theme. Original diagram text is preserved in `dataset.originalText` so re-renders start from source.

## MobileNavManager and MainSiteNavManager

`MobileNavManager` controls the document sidebar on mobile. A hamburger button (`#menu-toggle`) toggles the sidebar with an overlay backdrop. Dismissal: link click (mobile only, `innerWidth < 1024`), overlay click, click-outside, or Escape. Opening sets `data-mobile-menu-open="true"` on the body, which CSS uses to lock scrolling.

`MainSiteNavManager` handles the top-level site navigation hamburger, separate from the document sidebar. Same dismissal pattern, plus a `resize` listener that auto-closes at the 768px breakpoint. Menu state tracked via `dataset.expanded`.

Both managers check for their toggle elements before binding listeners -- on pages without those elements, they are no-ops.

## SidebarToggleManager

Handles the table of contents sidebar toggle for layouts with a collapsible right-side panel. Binds to `#sidebar-toggle`, `#sidebar-overlay`, `#sidebar-close`, and `#sidebar-panel`. The overlay click handler checks `e.target === this.sidebarOverlay` so clicks on the panel itself do not trigger a close. Escape key dismissal included.

## SearchManager

Client-side full-text search using FlexSearch with a modal interface. For a full walkthrough, see [Implement Search Functionality](xref:penn.guides.implement-search-functionality).

**Keyboard shortcut.** Cmd+K (macOS) or Ctrl+K (Windows/Linux) opens the search modal. Escape closes it.

**Lazy loading.** Neither the FlexSearch library nor the search index (`search-index.json`) is loaded at startup. Both are fetched on first modal open. If the load fails, `searchIndexFailed` prevents retry attempts.

**Index construction.** Documents are indexed into a FlexSearch `Document` instance with `tokenize: "forward"` and `LatinAdvanced` encoding. Two fields are indexed (`title`, `body`); four are stored for display (`title`, `body`, `url`, `section`).

**Weighted scoring.** `combineFieldResults()` merges results across fields. Title matches get 3x weight; body matches get 1x. Position-based scoring (`1 / (index + 1)`) and a per-document `priority` multiplier determine final ordering.

**Snippets and highlighting.** `getContentSnippet()` extracts a 150-character window centered on the first match. `highlightText()` wraps terms in `<mark class="search-highlight">` elements. Search is debounced at 300ms.

## Key Patterns

**Lazy loading.** External libraries load via dynamic `import()` only when needed: Mermaid on diagram detection, FlexSearch on modal open, highlight.js on unprocessed code blocks. `scripts.js` contains no library code.

**Conditional initialization.** Every manager checks for required DOM elements before binding listeners. A page without code tabs has no tab event handlers. A page without a search input never loads FlexSearch.

**Data attribute discovery.** Managers find elements via data attributes (`data-theme-toggle`, `data-role="page-outline"`, `data-content-selector`, `data-current`) rather than CSS classes, keeping JavaScript decoupled from styling.

**SPA integration.** After SPA navigation, the `spa:commit` event triggers reinitialization of content-dependent managers. `spa:before-navigate` clears the outline before new content arrives. Managers like `OutlineManager` abort their previous `AbortController` and set up fresh. Managers like `ThemeManager` and `SearchManager` operate on persistent chrome and are not reinitialized.

## Accessibility

- **Tabs**: `role="tablist"`, `role="tab"`, `role="tabpanel"`, `aria-controls`, managed `tabindex`.
- **Navigation**: `aria-expanded` on sidebar toggles, `aria-hidden` on overlays.
- **Search modal**: auto-focus input, body scroll lock.
- **Keyboard**: Escape closes every overlay, modal, and menu. Cmd/Ctrl+K opens search.
- **Screen readers**: the SPA engine maintains an `aria-live="polite"` announcer for page transitions.

## Performance

- **rAF throttle**: Scroll handler fires at most once per animation frame.
- **Passive listeners**: Scroll events use `{ passive: true }`.
- **AbortController cleanup**: Scroll listeners are aborted on reinit, preventing leaks across navigations.
- **Cached DOM queries**: Element references stored after `init()`, not re-queried per event.
- **Deferred loading**: CDN libraries fetched only when triggered.

The JavaScript exists to enhance server-rendered HTML. When it loads, interactive features appear. When it does not, every page still works.
