# DocSiteSectionsExample

Same DocSite host shape as the scaffold — the teaching surface is the structure of `Content/`. Two areas, each broken into two subfolder-backed sections, with `order:` / `sectionLabel:` front matter driving the sidebar grouping.

## Concepts

- **Subfolders become non-navigable section headers.** `Content/guides/getting-started/*.md` → "Getting Started" section header in the `/guides/` sidebar. Folder names are title-cased to humans by `NavigationBuilder.FormatSectionTitle` with a small built-in acronym list (api, cli, css, html, http, https, json, sdk, sql, svg, ui, url, xml, yaml, rss, pdf, png) — so `core-api/` becomes "Core API" rather than "Core Api".
- **Page ordering via `order:` front matter.** Pages sort by `Order` ascending (defaults to `int.MaxValue` when `order:` is absent), with `Title` as the tiebreaker. So three sibling pages — `installation.md` with `order: 10`, `configuration.md` with `order: 20`, and `first-project.md` with no `order:` — sort as installation → configuration → first-project, with the un-ordered page falling to the end alphabetically against any other un-ordered peers.
- **`sectionLabel:`** — same value as the folder-derived section header most of the time; setting it explicitly on a page overrides the folder-derived label for that specific page's row in the sidebar. The kitchen-sink files in this example carry `sectionLabel: Getting Started` / `sectionLabel: Advanced` to make the binding explicit; remove the front-matter line and the page still groups by its folder (so the override is opt-in, not required).
- **`NavigationBuilder` flattening** — folder hierarchy plus `order:` plus `sectionLabel:` produce one grouped tree handed to `MainLayout.razor`.

## Referenced from

- `docs/.../tutorials/docsite/sections-and-areas.md`
