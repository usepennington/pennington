# Post-mortem — DocSiteAuthorExample

> **Resolution (2026-04-14):** No actionable items — this postmortem reported
> "No blockers" and its findings are pure convention notes still valid for
> future DocSite apps.

## What was built

`examples/DocSiteAuthorExample/` — the second DocSite app. Same host shape as
app #4 (`AddDocSite` + `UseDocSite` + `RunDocSiteAsync`) trimmed to **one**
area (`Guides`) so the tutorial focuses on authoring, not area routing.
`Content/guides/index.md` is the landing page; `Content/guides/authoring.md`
is the teaching page with a fully-populated `DocSiteFrontMatter` block
(title, description, tags, section, order), a GitHub-style alert
(`> [!NOTE]`), a three-panel tabbed code group, and seven `##`/`###`
headings so the outline nav has something to list.

Stage files are static classes with `public static string Source()` helpers
— stage 1 bare front-matter + h1, stage 2 adds the alert, stage 3 adds the
tabbed code group. Returning the markdown as a string (rather than
instantiating a web host per stage) keeps the stages lightweight; tutorial
prose extracts each body with `csharp:xmldocid,bodyonly`.

## Verification

- `dotnet build Pennington.slnx` — clean, 0 errors.
- Dev server at `http://localhost:5499/guides/authoring` — Playwright
  confirmed chrome title "Authoring a doc page — Author Docs", rendered
  `<h1>`, `<meta name="description">` sourced from front matter, alert
  container with classes `markdown-alert markdown-alert-note`, a three-tab
  ARIA tablist (`dotnet CLI`, `PowerShell`, `csproj`) where the first panel
  was initially visible; clicking the `PowerShell` tab swapped the visible
  panel to `Install-Package Pennington`. Outline right-rail listed all
  seven headings in order.
- Static build — `build output` produced 8 pages; generated
  `authoring/index.html` contains `markdown-alert-note`, one `role="tablist"`,
  three `role="tab"` elements, and all seven heading anchor ids. Output
  cleaned.

## API reality / conventions for later DocSite apps (#6, #13)

- **Alert syntax.** GitHub-flavoured block quote with `[!KIND]` on the first
  non-whitespace line — nothing else. `CustomAlertInlineParser` replaces the
  surrounding `QuoteBlock` with an `AlertBlock` and adds
  `markdown-alert markdown-alert-<kind>` classes. Supported kinds:
  `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION`.
- **Tabbed code group syntax.** Mark **two or more adjacent** fenced code
  blocks with `tabs=true title="…"` in the fence's argument string (space-
  separated `key=value`/`key="value"` pairs after the language). A single
  `tabs=true` fence is left as a plain code block; consecutive ones collapse
  into one `TabbedCodeBlock` rendered as an ARIA tablist with sequential
  group IDs. The title defaults to the language name if omitted.
- **`DocSiteFrontMatter` field names match the spec exactly** — `Title`,
  `Description`, `Tags` (`string[]`), `Section`, `Order` (default
  `int.MaxValue`). Extra capabilities already wired: `IsDraft`, `Uid`,
  `Search`, `Llms`, `RedirectUrl`. No divergence.
- **Outline nav is JS-driven**, not server-side. `OutlineNavigation`
  renders a stub `<ul>` with `data-content-selector="article main"`;
  the `scripts.js` TOC builder walks the article on page load and inserts
  links. Works in dev and static output identically.

No blockers.
