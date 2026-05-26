---
title: "Cross-reference pages by uid"
description: "Give a page a stable `uid:` and link to it with `<xref:uid>` or `[text](xref:uid)` so links survive renames and moves."
uid: how-to.navigation.cross-references
order: 2
sectionLabel: "Navigation & Links"
tags: [xref, uid, linking, front-matter]
---

When relative `../foo/bar.md` links break every time a file moves or a section is renamed, assign each target page a stable `uid:` and link to it by name. Pennington resolves both link forms at request and build time, so moves do not break links.

## Before you begin
- A working Pennington site with markdown under `Content/` (see <xref:how-to.pages.front-matter> if not).
- Pages use `DocSiteFrontMatter` (or another type whose base `IFrontMatter` default member for `Uid` is preserved).
- The standard response-processing pipeline is active — `UsePennington` / `UseDocSite` wires the xref rewriter on every HTML response.

## Declare a `uid:` on the target page

Every page type already has a `uid:` field through `IFrontMatter` — filling it opts the page in. Choose a stable, dot-separated string that does not encode the current URL path.

```yaml
---
title: Cross-references (target)
uid: how-to.navigation.cross-references
---
```

## Link forms

### Inline `<xref:uid>`

Link text defaults to the target page's `Title`.

```markdown
See <xref:kitchen-sink.main.cross-references-b> for the other half of this pairing.
```

### Anchor-style `[text](xref:uid)`

Standard markdown link with a custom label.

```markdown
See the [cross-reference target page](xref:kitchen-sink.main.cross-references-b) for details.
```

## Verify

- Run `dotnet run`, visit the source page, and inspect the rendered HTML — both forms become ordinary `<a href="/canonical/path">` elements.
- Move or rename the target markdown file without touching the `uid:` — the next reload still resolves the link.
- Break a uid on purpose — the link renders with `data-xref-error="Reference not found"`, a warning appears in the dev diagnostic overlay, and `dotnet run -- build` surfaces it in the `BuildReport`.

## Related

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Background: [Cross-reference resolution](xref:explanation.routing.cross-references) — the two-phase resolver, ordering, and diagnostics.
