---
title: "Cross-reference pages by uid"
description: "Give a page a stable `uid:` and link to it with `<xref:uid>` or `[text](xref:uid)` so links survive renames and moves."
uid: how-to.content-authoring.cross-references
order: 100
sectionLabel: Content Authoring
tags: [xref, uid, linking, front-matter]
---

> **In this page.** _Paraphrase TOC "Covers": set `uid:` in front matter, link via `<xref:uid>` or `[text](xref:uid)`, and let `XrefHtmlRewriter` resolve the link at request/build time. Two sentences max._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": cross-referencing Roslyn symbols by xmldocid — that is a planned separate package (`Pennington.Roslyn`). Link out when that how-to lands; do not smuggle xmldocid link syntax into this page._

## When to use this

_Two sentences. Frame the arrival state: the reader already has a DocSite with several pages and is tired of relative `../foo/bar.md` links breaking every time a file moves or a section is renamed. They want a stable handle per page and a terse way to link to it by name. Do not re-teach front matter — point back to the front-matter how-to for YAML basics._

## Assumptions

_Keep to 3 bullets. Prior state must already include a working DocSite with at least two pages so the uid lookup is meaningful._

- You have a working Pennington site with markdown under `Content/` (see [Work with front matter](/how-to/content-authoring/front-matter) if not)
- Your pages use `DocSiteFrontMatter` (or another type whose base `IFrontMatter` default member for `Uid` is preserved)
- You are on the standard response-processing pipeline — i.e. `UsePennington` / `UseDocSite` is wired so `XrefHtmlRewriter` runs on every HTML response

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/cross-references-a.md` and `Content/main/cross-references-b.md` form a round-trip pairing. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four steps. Each opens with an imperative verb, shows the minimal markdown/yaml fence, and closes with one sentence on what the engine does in response. The two "link form" steps (2 and 3) split on syntax — the inline `<xref:…>` pre-parse phase versus the `[text](xref:…)` DOM phase — so the reader can pick the one that matches their voice._

### 1. Declare a `uid:` on the target page

_One sentence: `uid:` is a default member on `IFrontMatter` (collapsed there in commit `984dc7a`), so every page type already has it — you are only opting in by filling it. Pick a stable, namespaced string (dot-separated is the convention; see the kitchen-sink fixtures for `kitchen-sink.main.cross-references-b`) and resist the urge to encode the current URL, since the whole point is surviving a move._

```yaml
---
title: Cross-references (target)
uid: kitchen-sink.main.cross-references-b
---
```

_Backing contract for the `Uid` key:_

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.Uid
```

### 2. Link with the inline `<xref:uid>` form

_One sentence: the angle-bracket form reads like a one-liner pointer and is resolved in the pre-parse phase — the rewriter regex-replaces it before AngleSharp sees the document, because `<xref:…>` is not valid HTML and would otherwise be eaten by the parser. The link text defaults to the target page's `Title`._

```markdown
See <xref:kitchen-sink.main.cross-references-b> for the other half of this pairing.
```

_The pre-parse phase that handles this form:_

```csharp:xmldocid
M:Pennington.Infrastructure.XrefResolvingService.ResolveXrefTagsAsync(System.String,Pennington.Diagnostics.DiagnosticContext)
```

### 3. Link with the `[text](xref:uid)` form

_One sentence: the anchor-style form is a standard markdown link whose `href` happens to start with `xref:`, so Markdig emits a regular `<a>` and the DOM phase rewrites `href` (and optionally the text if it is still the raw `xref:…` placeholder) after parsing. Use this form whenever you want a custom link label._

```markdown
See the [cross-reference target page](xref:kitchen-sink.main.cross-references-b) for details.
```

_The DOM phase that handles this form:_

```csharp:xmldocid
M:Pennington.Infrastructure.XrefResolvingService.ResolveXrefLinksAsync(AngleSharp.Dom.IDocument,Pennington.Diagnostics.DiagnosticContext)
```

### 4. Let `XrefHtmlRewriter` resolve everything on response

_Two sentences. Both phases run inside `XrefHtmlRewriter` (`Order => 10`), which executes before `LocaleLinkHtmlRewriter` and `BaseUrlHtmlRewriter` so the canonical path it emits is what later rewriters see — identically in dev serve and `build`. The `uid → URL` map is owned by `XrefResolver`, built lazily from every `IContentService.GetCrossReferencesAsync()` and file-watched, so renaming or moving a target page invalidates the lookup without a restart._

_The rewriter that wires both phases into the response pipeline:_

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

_For a full round-trip pairing in the repo:_

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/cross-references-a.md
```

---

## Verify

_Terse. One bullet per form plus one for the failure path, so the reader can confirm success and recognize a broken xref without reading anything else._

- Run `dotnet run`; visit the source page and inspect the rendered HTML — both `<xref:…>` and `[text](xref:…)` have become ordinary `<a href="/canonical/path">` elements
- Move or rename the target markdown file without touching the `uid:`; the next reload still resolves the link (file-watched `XrefResolver` rebuilds the lookup)
- Break a uid on purpose; the link renders with `data-xref-error="Reference not found"`, a warning appears in the dev diagnostic overlay, and `dotnet run -- build` surfaces it in the `BuildReport`

## Related

_Two to four cross-quadrant links. Point at Reference for the full rewriter catalog and the front-matter key table, and at Explanation for the resolution model. Do not link to the next how-to in this section — generated automatically._

- Reference: [Response processing interfaces](/reference/extension-points/response-processing)
- Reference: [Front matter key reference](/reference/front-matter/keys)
- Background: [Cross-reference resolution](/explanation/routing/cross-references)
