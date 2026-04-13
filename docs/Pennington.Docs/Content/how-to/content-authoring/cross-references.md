---
title: "Cross-reference pages by uid"
description: "Link to pages by uid using <xref:uid> (auto-titled) or [text](xref:uid) (custom link text); XrefHtmlRewriter resolves at request / build time."
section: "content-authoring"
order: 80
tags: []
uid: how-to.content-authoring.cross-references
isDraft: true
search: false
llms: false
---

> **In this page.** Setting `uid:` in front matter and linking to the page from elsewhere by `uid` — either with the self-closing `<xref:uid>` tag (title auto-filled from the target) or the Markdown link form `[text](xref:uid)` (custom anchor text). How `XrefHtmlRewriter` resolves the link at request / build time and how broken xrefs surface in the build report.
>
> **Not in this page.** Cross-referencing Roslyn symbols by xmldocid — that is a planned separate package (`Pennington.Roslyn` symbol xrefs).

## When to use this

- You want a stable, rename-safe link to another Pennington page — one that keeps working when the target's URL changes.
- You want to reference a page from many places and let `XrefHtmlRewriter` keep every link in sync if you rename the file or move it under a different section.
- You want the build to flag broken internal links explicitly (via `BuildReport`) rather than silently shipping a 404.

## Assumptions

- Bullets to cover under Assumptions:
- You have an existing Pennington site wired with `AddPennington` + `UsePennington` (any template).
- You are comfortable editing YAML front matter and Markdown link syntax.
- You know which two syntaxes Pennington actually resolves:
  1. **Self-closing tag form** — `<xref:uid>` (no attributes, no closing tag, no custom text). The anchor text is auto-filled from the resolved page's `title`.
  2. **Markdown-link form** — `[text](xref:uid)`, which renders as `<a href="xref:uid">text</a>` and is rewritten in the DOM phase. This is the only form that supports custom link text.
- Note the form `<xref uid="…">text</xref>` does **not** exist in Pennington. Don't author it — the regex matched by `XrefResolvingService.XrefTagRegex()` is `<xref:([^>]+)>`.

To copy a working setup, see [`examples/BeaconDocsExample`](https://github.com/scottsauber/Penn/tree/main/examples/BeaconDocsExample) — its `Content/guides/configuration.md` declares a `uid`, and other pages link to it via both syntaxes.

---

## Steps

### 1. Give the target page a `uid`

- Add `uid: "your.stable.id"` to the target page's YAML front matter.
- Naming tip: use a dotted, kebab-lowercase identifier you will not rename (`beacon.api-reference`, `blogsite.tutorial.first-post`) — uids are matched case-insensitively by `XrefResolver`.
- The `uid` is project-wide — collisions across locales are deduped; the first registered wins.

```yaml
---
title: "API reference"
uid: "beacon.api-reference"
---
```

### 2. Link to it from another page — self-closing tag form

- Use `<xref:uid>` inline in any markdown file. No closing tag, no attributes.
- The anchor text is auto-filled from the target's `title` at render time.
- Use this when you want the link text to mirror the target's title and to stay in sync if the target is retitled.

```markdown
For the full API surface see <xref:beacon.api-reference>.
```

### 3. Link to it with custom anchor text — Markdown-link form

- Use `[your text](xref:uid)`. Any text inside the `[...]` is preserved verbatim as the anchor text.
- Use this when the surrounding sentence reads better with a natural phrase than the target title.

```markdown
Full details are in the [Beacon API reference](xref:beacon.api-reference).
```

### 4. Rebuild and let `XrefHtmlRewriter` resolve the link

- `XrefHtmlRewriter` runs as one of the three built-in `IHtmlResponseRewriter`s in the single AngleSharp pass. No extra wiring is needed.
- On resolution, `<xref:uid>` is replaced by an `<a href="…">title</a>` element; `<a href="xref:uid">text</a>` keeps its original text and has its `href` rewritten.
- Broken xrefs (no page with that `uid`) are left intact in HTML and surface as `BrokenLink` entries on the `BuildReport` after `build`.

### 5. Verify the link resolves

- Run `dotnet run` and open the source page in a browser.
- Inspect the rendered link — the `href` should point to the target page's canonical URL.
- Run a `build` and scan the `BuildReport` — any xref whose `uid` didn't resolve will appear under broken links.

---

## Verify

- Run `dotnet run` and hover the link on the source page — the status bar should show the target's real URL, not the literal string `xref:<uid>`.
- For the self-closing form, confirm the anchor text equals the target's `title`.
- For the Markdown-link form, confirm the anchor text is your custom text and the `href` points to the target's canonical URL.
- Run `dotnet run -- build <baseUrl> output` and confirm `BuildReport` reports zero broken links for the pages you linked.

## Related

- Related reference: [Response processing interfaces](/reference/extension-points/response-processing) — `IHtmlResponseRewriter`, `XrefHtmlRewriter`, and rewriter execution order.
- Related reference: [Front matter key reference](/reference/front-matter/keys) — the `uid` key.
- Background: [Cross-reference resolution](/explanation/routing/cross-references) — how uids are gathered during the content phase and resolved during the response phase.
