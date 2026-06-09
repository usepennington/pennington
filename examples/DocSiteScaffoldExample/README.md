# DocSiteScaffoldExample

Wires the DocSite template — `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` — onto an empty ASP.NET project. A `Content/guides/` folder of markdown pages shows the sidebar building itself from the shape of `Content/`, with no areas and no per-page `sectionLabel`.

## Concepts

- `AddDocSite` template wiring (Razor layout, sidebar, header, search, outline, dark mode)
- Folder-driven navigation — markdown under `Content/` becomes sidebar entries; a subfolder becomes a navigation group, sorted by `order:`
- A root `Content/index.md` serving `/`
- A root `Content/404.md` — the not-found body. It is reserved out of discovery (no `/404/` route, not in nav/sitemap/search), rendered by the catch-all for any unmatched URL, and written to `output/404.html` by the static build.
- `UseDocSite` ordering (locale → antiforgery → static files → routing → MonorailCSS → SPA → Pennington middleware)

## Referenced from

- `docs/.../tutorials/docsite/scaffold.md`
- `docs/.../reference/host/extensions.md` (Program.cs fence)
- `docs/.../how-to/pages/not-found-page.md` (`Content/404.md` fence)
