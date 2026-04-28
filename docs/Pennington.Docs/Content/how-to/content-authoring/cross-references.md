---
title: "Cross-reference pages by uid"
description: "Give a page a stable `uid:` and link to it with `<xref:uid>` or `[text](xref:uid)` so links survive renames and moves."
uid: how-to.content-authoring.cross-references
order: 201100
sectionLabel: Content Authoring
tags: [xref, uid, linking, front-matter]
---

When relative `../foo/bar.md` links break every time a file moves or a section is renamed, assign each target page a stable `uid:` and link to it by name. `XrefHtmlRewriter` resolves both link forms at request and build time, so moves do not break links.

## Assumptions

- A working Pennington site with markdown under `Content/` (see <xref:how-to.content-authoring.front-matter> if not).
- Pages use `DocSiteFrontMatter` (or another type whose base `IFrontMatter` default member for `Uid` is preserved).
- The standard response-processing pipeline is active — `UsePennington` / `UseDocSite` wires `XrefHtmlRewriter` to run on every HTML response.

## Declare a `uid:` on the target page

Every page type already has a `uid:` field through `IFrontMatter` — filling it opts the page in. Choose a stable, dot-separated string that does not encode the current URL path; the value of a uid is that it survives a move.

```yaml
---
title: Cross-references (target)
uid: kitchen-sink.main.cross-references-b
---
```

The backing front-matter contract:

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.Uid
```

## Link forms

### Inline `<xref:uid>` form

The angle-bracket form resolves in the pre-parse phase — `XrefResolvingService` regex-replaces it before AngleSharp sees the document, because `<xref:…>` is not valid HTML and the parser would swallow it. Link text defaults to the target page's `Title`.

```markdown
See <xref:kitchen-sink.main.cross-references-b> for the other half of this pairing.
```

The pre-parse phase that handles this form:

```csharp:xmldocid
M:Pennington.Infrastructure.XrefResolvingService.ResolveXrefTagsAsync(System.String,Pennington.Diagnostics.DiagnosticContext)
```

### Anchor-style `[text](xref:uid)` form

The anchor-style form is a standard markdown link whose `href` starts with `xref:`. Markdig emits a regular `<a>` and the DOM phase rewrites the `href` after parsing. This form carries a custom link label.

```markdown
See the [cross-reference target page](xref:kitchen-sink.main.cross-references-b) for details.
```

The DOM phase that handles this form:

```csharp:xmldocid
M:Pennington.Infrastructure.XrefResolvingService.ResolveXrefLinksAsync(AngleSharp.Dom.IDocument,Pennington.Diagnostics.DiagnosticContext)
```

## How resolution works

Both phases run inside `XrefHtmlRewriter` (`Order => 10`), which executes before `LocaleLinkHtmlRewriter` and `BaseUrlHtmlRewriter` so later rewriters see canonical paths — identically in dev serve and `build`. `XrefResolver` owns the `uid → URL` map, built lazily from `IContentService.GetCrossReferencesAsync()` and file-watched, so moving or renaming a target page invalidates the lookup without a restart.

The rewriter that wires both phases into the response pipeline:

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

A round-trip pairing lives in the repo at `examples/DocSiteKitchenSinkExample/Content/main/cross-references-a.md` (with its sibling `cross-references-b.md`).

## Verify

- Run `dotnet run`; visit the source page and inspect the rendered HTML — both `<xref:…>` and `[text](xref:…)` become ordinary `<a href="/canonical/path">` elements.
- Move or rename the target markdown file without touching the `uid:` — the next reload still resolves the link (file-watched `XrefResolver` rebuilds the lookup).
- Break a uid on purpose — the link renders with `data-xref-error="Reference not found"`, a warning appears in the dev diagnostic overlay, and `dotnet run -- build` surfaces it in the `BuildReport`.

## Related

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Background: [Cross-reference resolution](xref:explanation.routing.cross-references)
