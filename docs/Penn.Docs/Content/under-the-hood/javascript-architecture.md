---
title: "JavaScript Architecture"
description: "The modular client-side JavaScript system that powers Penn's interactive features"
uid: "penn.under-the-hood.javascript-architecture"
order: 3050
---

Penn generates static HTML. Every page works without JavaScript -- search engines index it, browsers render it, screen readers read it. But a documentation site with zero interactivity is... spartan. So Penn ships `scripts.js`, a modular client-side system that adds theme switching, search, code tabs, page outlines, and other quality-of-life features.

The architecture follows a simple principle: each feature is a self-contained manager class, and `PageManager` coordinates them all. Nothing initializes unless the DOM elements it needs are present. Nothing loads external libraries until they are actually needed.

## Architecture Overview

```
PageManager (entry point, window.pageManager)
  |
  +-- ThemeManager         -- dark/light mode switching
  +-- OutlineManager       -- scroll-tracking table of contents
  +-- TabManager           -- tabbed code blocks with ARIA support
  +-- MermaidManager       -- diagram rendering with theme awareness
  +-- MobileNavManager     -- hamburger menu for mobile
  +-- MainSiteNavManager   -- top-level site navigation
  +-- SidebarToggleManager -- ToC sidebar panel
  +-- SearchManager        -- FlexSearch-powered client-side search
```

## PageManager

The coordinator. `PageManager` detects DOM ready state, instantiates all component managers, and exposes itself as `window.pageManager` for debugging. It handles the lifecycle: initialize on page load, reinitialize after SPA navigation (via the `spa:commit` event).

Not much to say about it. It creates managers and calls their `init()` methods. The boring kind of important.

## ThemeManager

Manages dark/light theme switching with `localStorage` persistence. Binds to any element with the `data-theme-toggle` attribute. When toggled, it:

1. Flips the `dark` class on the document root.
2. Saves the preference to `localStorage`.
3. Notifies other managers (like MermaidManager) to update their theme-dependent rendering.

Theme preference is read on page load before first paint, avoiding the dreaded white flash when the user prefers dark mode. This is achieved by an inline `<script>` in the `<head>` that runs before the body renders.

## OutlineManager

Provides a scroll-tracking table of contents in the sidebar. As you scroll through the page, the currently visible heading is highlighted in the outline.

The implementation uses `requestAnimationFrame`-throttled scroll detection (not `IntersectionObserver` -- the scroll math is simpler and gives more predictable results for the "which heading is currently at the top" question). A visual highlighter element tracks the current section with smooth CSS transitions.

OutlineManager can either use pre-rendered outline markup (from the server-side `RenderedContent.Outline` data) or dynamically generate an outline from H2/H3 headings in the content. The dynamic generation is a fallback for pages that do not have server-rendered outlines.

Configuration via data attributes:

- `data-role="page-outline"` -- the outline navigation container
- `data-content-selector` -- CSS selector for the content element to scan for headings

## TabManager

Handles tabbed code blocks -- the ones where you can switch between multiple languages or configurations in a single code block group. Full ARIA accessibility support: `role="tablist"`, `role="tab"`, `role="tabpanel"`, `aria-selected`, and keyboard navigation.

Tab state is synchronized across groups on the same page. If you select "bash" in one tab group, all other groups that have a "bash" tab switch to it too. This is stored in `sessionStorage` so it persists across page navigations within a session.

Each tab group is identified by a pattern in its ID, allowing multiple independent tab groups on the same page.

## MermaidManager

Renders Mermaid diagrams with theme-aware styling. Diagrams are authored in markdown as `mermaid` fenced code blocks and rendered client-side by the Mermaid library.

The interesting bit: Penn's CSS uses OKLCH color variables (via MonorailCSS), but Mermaid only understands hex colors. `MermaidManager` reads the computed CSS custom properties using `getComputedStyle()`, converts OKLCH values to hex, and passes them as Mermaid theme configuration.

When the theme changes (dark to light or vice versa), `ThemeManager` notifies `MermaidManager`, which re-renders all diagrams with the new color scheme. This avoids the jarring experience of dark-themed diagrams on a light page.

Mermaid itself is lazy-loaded from CDN. If no `mermaid` code blocks exist on the page, the library is never fetched.

## MobileNavManager

Controls the mobile navigation sidebar. A hamburger button toggles the sidebar, which slides in from the left with an overlay backdrop. Includes:

- Auto-close on link click (so navigating closes the menu)
- Escape key support
- Body scroll locking when the menu is open
- Click-outside-to-close

At desktop breakpoints, the mobile nav is hidden entirely via CSS. The manager does not initialize its event listeners unnecessarily -- it checks for the toggle button element first.

## MainSiteNavManager

Manages the top-level site navigation menu, primarily for mobile viewports. Auto-closes when the window resizes past the desktop breakpoint (768px). Includes click-outside and escape key handlers.

## SidebarToggleManager

Handles the table of contents sidebar toggle for layouts with collapsible sidebars. Provides an overlay-based sidebar with a close button. Event propagation is stopped when clicking inside the panel, so it does not accidentally close itself.

## SearchManager

Client-side search using FlexSearch with a modal interface. This is the most complex manager, and it earns the complexity:

- **Keyboard shortcut**: Cmd/Ctrl+K opens the search modal.
- **Lazy loading**: Both the search index (`search-index.json`) and the FlexSearch library are loaded on first open, not at page load.
- **Weighted scoring**: Title matches rank higher than description, which ranks higher than heading matches, which rank higher than body content matches.
- **Query highlighting**: Search terms are highlighted in results.
- **Snippet generation**: Results show a text snippet centered around the matched term.
- **Error handling**: Network failures and parsing errors are caught and reported gracefully.

The search index is generated at build time by the content pipeline. Each `RenderedItem` can include a `SearchIndexDocument` with title, description, headings, and body text. The `SearchIndexBuilder` aggregates these into a single JSON file.

## Key Patterns

### Lazy Loading

External libraries are loaded dynamically from CDN only when needed:

- **Mermaid**: Loaded when a `mermaid` code block is detected.
- **FlexSearch**: Loaded when the search modal opens.

This keeps the initial page load fast. `scripts.js` itself is small; the heavy libraries only arrive when their features are actually used.

### Conditional Initialization

Every manager checks for its required DOM elements before initializing:

```javascript
init() {
    this.toggleButton = document.querySelector('[data-mobile-nav-toggle]');
    if (!this.toggleButton) return; // Nothing to do on this page
    // ... set up event listeners
}
```

This means a page without code tabs does not have tab event listeners. A page without a search button does not load FlexSearch. Features are present only when the HTML calls for them.

### SPA Navigation Integration

After an SPA navigation (see [SPA Island Architecture](xref:penn.under-the-hood.spa-island-architecture)), the `spa:commit` event fires on `document`. `PageManager` listens for this event and reinitializes all managers for the new page content. This handles:

- Rebuilding the page outline from new headings
- Initializing code tabs in the new content
- Rendering any new Mermaid diagrams
- Updating the active navigation link

The reinit is lightweight -- managers clean up their old state and set up fresh for the new DOM.

### Data Attribute Configuration

Managers discover their DOM elements through data attributes rather than CSS class names. This keeps the JavaScript decoupled from styling:

- `data-theme-toggle` -- theme toggle buttons
- `data-role="page-outline"` -- outline container
- `data-content-selector` -- content element for outline generation
- `data-spa-island` -- SPA island containers

## Accessibility

All interactive components follow accessibility best practices:

- **ARIA attributes** for tabs (`tablist`, `tab`, `tabpanel`), navigation landmarks, and modals.
- **Keyboard navigation** with Escape key support for modals, overlays, and menus.
- **Focus management** in the search modal (auto-focus on the input, trap focus within the modal).
- **Screen reader support** through semantic HTML, ARIA labels, and live regions.

## Performance Notes

- Scroll handlers use `requestAnimationFrame` throttling.
- Event listeners use `{ passive: true }` where appropriate (scroll events).
- DOM queries are cached after initialization, not repeated on every event.
- External library loading is deferred and conditional.

The JavaScript footprint is intentionally small. Penn is a static documentation site generator, not a single-page application framework. The JS exists to enhance, not to replace, the server-rendered HTML.
