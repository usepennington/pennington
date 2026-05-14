# DocSiteKitchenSinkExample

Kitchen-sink DocSite host. The configuration surface is deliberately wide — two areas, two locales, a custom color scheme, font preloads, extra CSS, a bespoke Mdazor component (`FeatureCallout`), a custom footer, and a GitHub URL. Each how-to page xmldocid-fences into a small helper method on `ServiceConfiguration` to show exactly the surface that how-to teaches.

## Concepts (the union of)

- **Sidebar customization** — `sectionLabel:` + `order:` front matter drive grouping and order; the sidebar itself doesn't currently ship a "hide from sidebar" front-matter switch (the two opt-out demos below address search and llms.txt, *not* the sidebar). Render the running example and inspect `nav#nav-sidebar` to see the structure produced from the kitchen-sink `Content/` layout. Redirects are demonstrated via `redirect-source.md` (front-matter `redirectUrl:`).
- **Cross-references** — `xref:uid` round-trip pairings; every kitchen-sink page sets `uid:` so the xref resolver can name it.
- **MonorailCSS theming** — color scheme, font preloads, extra CSS (see `ServiceConfiguration.cs`).
- **`<FeatureCallout />`** — registering a custom Mdazor component, rendered at `/main/ui-components-in-markdown/`.
- **Front-matter examples** — built-in `DocSiteFrontMatter` everywhere, plus a custom `ApiFrontMatter` record on `Content/api/*`.
- **Search-index opt-out** — `Content/main/hidden.md` carries `search: false`. The page still renders at its URL and still appears in the sidebar; only the entry in `/search-index-en.json` is omitted (open the JSON and search for "Not in search" — it isn't there).
- **`llms.txt` opt-out** — `Content/main/llms-hidden.md` carries `llms: false`. The page renders and appears in the sidebar; `/llms.txt` skips it (`curl /llms.txt | grep "Not in llms.txt"` returns nothing).
- **Multi-area `Content/` layout** — `Content/main/` and `Content/api/` produce two top-level areas; `Content/fr/` provides the localized tree.

## Referenced from

- `docs/.../how-to/navigation/customize-sidebar.md`
- `docs/.../how-to/navigation/cross-references.md`
- `docs/.../how-to/theming/monorail-css.md`
- `docs/.../how-to/theming/fonts.md`
- `docs/.../how-to/discovery/search.md`
- `docs/.../how-to/discovery/multiple-sources.md`
- `docs/.../how-to/pages/front-matter.md`
- `docs/.../how-to/pages/redirects.md`
- `docs/.../how-to/feeds/llms-txt.md`
- `docs/.../how-to/rich-content/ui-components-in-markdown.md`
- `docs/.../reference/front-matter/keys.md`
