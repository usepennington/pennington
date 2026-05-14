# DocSiteAuthorExample

Single-area DocSite focused on *authoring* a page — `DocSiteFrontMatter`, alerts, tabbed code groups, and the outline populated from rendered headings. Area routing is deliberately minimal so the tutorial can stay on page-level concerns.

## Concepts

- `DocSiteFrontMatter` — every shipped key is exercised on `Content/guides/authoring.md` (`title`, `description`, `uid: guides.authoring`, `tags`, `sectionLabel`, `order`); a reader inspecting that page sees a worked example of each.
- Markdown extensions: alerts, tab groups — full reference at [reference/markdown/extensions.md](../../docs/Pennington.Docs/Content/reference/markdown/extensions.md) (the same page that fences stage2/stage3 of this example).
- Outline nav generated from `h2`/`h3` in the rendered HTML — renders into `<div data-spa-region="outline" class="hidden xl:block …">`, so viewports below the Tailwind `xl` breakpoint (1280 px) hide the rail by design. At >=1280 px the right rail shows a "On this page" list of anchor links.

## Staged content

Markdown stages live under `snippets/` as `stage1.md` → `stage2.md` → `stage3.md` (pulled by `markdown:path` fences — xmldocid is C#/VB only).

## Referenced from

- `docs/.../tutorials/docsite/first-doc-page.md`
- `docs/.../reference/markdown/extensions.md` (stage2.md, stage3.md fences)
