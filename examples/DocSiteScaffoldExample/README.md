# DocSiteScaffoldExample

Wires the DocSite template — `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` — onto an empty ASP.NET project. A `Content/guides/` folder of markdown pages shows the sidebar building itself from the shape of `Content/`, with no areas and no per-page `sectionLabel`.

## Concepts

- `AddDocSite` template wiring (Razor layout, sidebar, header, search, outline, dark mode)
- Folder-driven navigation — markdown under `Content/` becomes sidebar entries; a subfolder becomes a navigation group, sorted by `order:`
- A root `Content/index.md` serving `/`
- `UseDocSite` ordering (locale → antiforgery → static files → routing → MonorailCSS → SPA → Pennington middleware)

## Referenced from

- `docs/.../tutorials/docsite/scaffold.md`
- `docs/.../reference/host/extensions.md` (Program.cs fence)
