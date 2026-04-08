# P2: Font Preload Hints

## Problem
The doc site uses custom fonts loaded via `@font-face` in CSS with `font-display: swap`, but the HTML contains no `<link rel="preload">` hints. This means fonts aren't discovered until the CSS is parsed, causing a flash of unstyled text (FOUT) that could be mitigated by preloading.

## Current State
- `App.razor` (`src/Penn.DocSite/Components/App.razor`) has the `<head>` section with stylesheet and script references
- `DocSiteOptions` has `DisplayFontFamily` and `BodyFontFamily` properties but no font file path configuration
- Font files are served from the wwwroot or RCL static assets
- No `<link rel="preload">` elements exist anywhere in the head

## Requirements
- Add a way to configure font file paths in `DocSiteOptions` (list of font URLs to preload, with their MIME types — typically `font/woff2`)
- Emit `<link rel="preload" href="..." as="font" type="font/woff2" crossorigin>` in `App.razor` for each configured font
- Preload hints must appear before the stylesheet `<link>` to be effective
- The `crossorigin` attribute is required even for same-origin fonts (browser requirement for font preloading)
- If no fonts are configured, emit nothing (backward compatible)
- Consider auto-detecting font files from `@font-face` declarations in the MonorailCSS output as a future enhancement, but for now explicit configuration is sufficient

## Key Files
- `src/Penn.DocSite/DocSiteOptions.cs` — add font preload configuration
- `src/Penn.DocSite/Components/App.razor` — emit preload links in head
