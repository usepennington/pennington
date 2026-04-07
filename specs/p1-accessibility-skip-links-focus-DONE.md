# P1: Skip Links and SPA Focus Management

## Problem
The site lacks skip navigation links for keyboard/screen reader users. The SPA engine (`spa-engine.js`) already handles focus after navigation (finds first heading, sets `tabindex="-1"`, focuses it) and has an ARIA live region for announcements, but the initial page load has no skip links and the main layout lacks ARIA landmark labels.

## Requirements

### Skip Links
- Add a visually-hidden skip link as the first focusable element in `MainLayout.razor` that jumps to the `<article>` content area
- The link should become visible on focus (standard pattern: `sr-only focus:not-sr-only`)
- Target should be the `<article data-spa-island="content">` element — give it an `id` if it doesn't have one
- Consider a second skip link to the sidebar navigation (`#nav-sidebar`)

### Landmark Labels
- Add `aria-label` attributes to distinguish the multiple `<nav>` elements in MainLayout (sidebar nav vs header nav)
- The `<main>` element wrapping the content area should be a semantic `<main>` (currently it's a `<main>` tag — verify it's the outermost one)

### SPA Focus Verification
- The existing `spa-engine.js` focus management (heading focus after navigation) is good — verify it works when no heading exists in the content (fallback to island element is already implemented)
- Ensure the ARIA live region announcement timing doesn't conflict with the focus change

## Key Files
- `src/Penn.DocSite/Components/Layout/MainLayout.razor` — add skip links and landmark labels
- `src/Penn.UI/wwwroot/spa-engine.js` — verify focus management edge cases (no changes expected)
- MonorailCSS or inline styles for the visually-hidden skip link styling
