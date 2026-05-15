# DocSitePagesAndLinksExample

Single-area DocSite carrying two content pages (`install`, `configure`) and a hub `index` that links to both. The example demonstrates the three link forms a real docs site reaches for: relative paths between siblings, absolute paths from a hub, and `uid:`-based cross-references that survive page renames. It also carries `Components/Index.razor` — a Razor landing page routed at `/`.

## Concepts

- Adding markdown files under an area folder — sidebar entries appear in `order:` sequence.
- **Relative paths** (`[Configure the site](./configure)`) — the right shape for sibling pages that stay co-located.
- **Absolute paths** (`[Configure the site](/guides/configure)`) — stable across folder moves of the source, used here on the hub index.
- **`uid:` + `xref:`** — `Content/guides/install.md` carries `uid: guides.install`, and the hub index reaches it via `[Install Pennington](xref:guides.install)`. The link survives renaming the file.
- **Razor landing page** — `Components/Index.razor` carries `@page "/"` and `@layout ...FullWidthLayout`, owning the site root with a sidebar-free marketing layout.

## Auxiliary snippets

`snippets/markdown-alert-example.md` and `snippets/markdown-tabs-example.md` back the alerts and tabs sections of `docs/.../reference/markdown/extensions.md`. They are not part of the linking tutorial's flow.

## Referenced from

- `docs/.../tutorials/docsite/first-doc-page.md`
- `docs/.../tutorials/docsite/landing-page.md` (`Components/Index.razor`)
- `docs/.../reference/markdown/extensions.md` (alert + tabs snippets)
