# DocSiteScaffoldExample

Swaps the bare `AddPennington` host for the DocSite template — `AddDocSite` / `UseDocSite` / `RunDocSiteAsync`. Two areas (`Guides`, `Reference`) demonstrate the sidebar area selector with the minimum amount of content.

## Concepts

- `AddDocSite` template wiring (Razor layout, sidebar, header, search, outline, dark mode)
- `ContentArea(label, folder)` mapping a top-level `Content/<folder>` to a URL prefix
- `UseDocSite` ordering (locale → antiforgery → static files → routing → MonorailCSS → SPA → Pennington middleware)

## Referenced from

- `docs/.../tutorials/docsite/scaffold.md`
- `docs/.../reference/host/extensions.md` (Program.cs fence)
