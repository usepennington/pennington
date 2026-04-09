# P3: Link Verification Enhancement in Static Build

## Problem
`LinkVerificationService` currently performs internal link checking during static build (Phase 9 of `OutputGenerationService`), but it only verifies that target URLs exist in the known route set. It does not verify external links, check for anchor/fragment validity, or validate image references resolve to actual files.

## Current State
- `LinkVerificationService` (`src/Penn/Infrastructure/LinkVerificationService.cs`) extracts `href` and `src` attributes via regex, classifies links as `ValidLink`, `ExternalLink`, or `BrokenLinkResult`
- External links are classified but never verified (no HTTP requests)
- Anchor-only links (`#fragment`) are auto-valid with no heading ID verification
- Framework paths (`/_content/`, `/_framework/`, `/_blazor/`) are auto-valid
- Results are reported in `BuildReport` as `BrokenLink` entries
- `BuildReport` (`src/Penn/Generation/BuildReport.cs`) already has `BrokenLinks` collection

## Requirements

### External Link Verification
- Add an optional mode (off by default) to verify external links during static build via HTTP HEAD requests
- Use configurable concurrency limits and timeouts to avoid hammering external servers
- Cache results within a build run to avoid re-checking the same URL
- Report broken external links (4xx/5xx, timeouts, DNS failures) as warnings, not errors — external sites may be temporarily down
- Add configuration in `OutputOptions` or `PennOptions` to enable/disable and set timeout

### Anchor Fragment Verification
- When a link targets `#fragment` on an internal page, verify that the rendered HTML for the target page contains an element with `id="fragment"`
- This requires cross-referencing the fetched HTML content (already captured in `FetchResult.HtmlContent`) — extract all `id` attributes per page and check anchors against them
- Report as warnings

### Image/Asset Verification
- Links classified as `LinkType.Image` (from `src` attributes) that point to internal paths should be verified against the output directory after static assets are copied (Phase 4)
- Check that the referenced file was actually written to the output

### Build Report Enhancement
- Distinguish between internal broken links (errors) and external/anchor issues (warnings) in the build report
- Add a summary count to the build report output

## Key Files
- `src/Penn/Infrastructure/LinkVerificationService.cs` — extend with new verification modes
- `src/Penn/Generation/OutputGenerationService.cs` — wire enhanced verification into Phase 9
- `src/Penn/Generation/BuildReport.cs` — may need new result categories
- `src/Penn/Infrastructure/PennOptions.cs` or `OutputOptions` — configuration for external checking
