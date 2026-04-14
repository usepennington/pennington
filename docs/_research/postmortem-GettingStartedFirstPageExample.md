# Post-mortem — GettingStartedFirstPageExample

## What was built

`examples/GettingStartedFirstPageExample/` — a three-page markdown site that
backs tutorial §1.1.20. Reuses the bare-host shape app #1 established
(`AddPennington` + `UsePennington` + `MapGet("/{*path}", ...)` iterating
`IContentService`) and adds a `NavigationBuilder.BuildTree(...)` call so every
page renders a tiny nav strip. Content is three markdown files with `title:`
front matter — `index.md`, `about.md` (`order: 20`), `contact.md`
(`order: 30`) — mapping to `/`, `/about`, and `/contact`. Three stage classes
capture the one-file → two-file → three-file progression; stages 2 and 3
delegate to `Stage1.Run` because the point of the tutorial is that adding
markdown files requires **zero** code change.

## Verification

- `dotnet build Pennington.slnx` — succeeds, 0 new warnings/errors.
- Dev server — Playwright confirmed `http://localhost:5466/` ("Welcome"),
  `/about` ("About"), and `/contact` ("Contact") each render with their
  distinct front-matter title plus markdown body. Nav strip lists About then
  Contact in that order on every page.
- Static build — `dotnet run -- build output` reports
  `Build Complete — 5 pages in 0.4s` (three HTML pages, sitemap.xml,
  search-index-en.json) with no errors. `output/` cleaned afterward.

## API surprises / follow-ups

- **`NavigationBuilder.BuildTree` needs explicit TOC input.** It does not pull
  from DI on its own — the caller loops `IContentService.GetIndexableEntriesAsync()`
  and concatenates the lists. Worth recording for the next agent who wants an
  auto-nav on a bare host.
- **Root `index.md` is not emitted as a nav entry** because its hierarchy is
  empty and `BuildLevel` filters by depth; only sibling files show up. That is
  acceptable for this tutorial (home + two siblings is fine), but later
  examples that want an explicit "Home" link will need a different strategy.

No blockers.
