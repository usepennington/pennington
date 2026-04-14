---
title: "Cross-reference pages by uid"
description: "Link to pages by `uid` using `<xref:uid>` (auto-titled) or `[text](xref:uid)` (custom link text)."
section: "content-authoring"
order: 100
tags: []
uid: how-to.content-authoring.cross-references
isDraft: true
search: false
llms: false
---

> **In this page.** Setting `uid:` in front matter, linking via `<xref:uid>` or `[text](xref:uid)`, and letting `XrefHtmlRewriter` resolve links at request/build time.
>
> **Not in this page.** Cross-referencing Roslyn symbols by xmldocid — that is a planned separate package (`Pennington.Roslyn`).

## When to use this

When you want a stable, rename-safe link to another Pennington page — one that keeps working when the target's URL changes. Broken xrefs are flagged explicitly in the build report rather than silently shipping a 404.

## Assumptions

- You have an existing Pennington site wired with `AddPennington` + `UsePennington` (any template).
- You are comfortable editing YAML front matter and Markdown link syntax.

Pennington resolves two xref syntaxes:

1. **Self-closing tag** — `<xref:uid>` (no attributes, no closing tag). The anchor text is auto-filled from the target's `title`.
2. **Markdown-link form** — `[text](xref:uid)`. This is the only form that supports custom anchor text.

The form `<xref uid="…">text</xref>` does not exist in Pennington — don't author it.

To copy a working setup, see [`examples/BeaconDocsExample`](https://github.com/usepennington/pennington/tree/main/examples/BeaconDocsExample) — its `Content/guides/configuration.md` declares a `uid` and other pages link to it via both syntaxes.

---

## Steps

### 1. Give the target page a `uid`

Add `uid: "your.stable.id"` to the target page's YAML front matter. Use a dotted, kebab-lowercase identifier you will not rename (`beacon.api-reference`, `blogsite.tutorial.first-post`) — uids are matched case-insensitively. The `uid` is project-wide; collisions across locales are deduped and the first registered wins.

```yaml
---
title: "API reference"
uid: "beacon.api-reference"
---
```

### 2. Link to it with the self-closing tag form

Use `<xref:uid>` inline in any markdown file. The anchor text is auto-filled from the target's `title` at render time — use this when you want the link text to stay in sync with the target title.

```markdown
For the full API surface see <xref:beacon.api-reference>.
```

### 3. Link to it with custom anchor text

Use `[your text](xref:uid)`. Any text inside `[...]` is preserved verbatim as the anchor text. Reach for this when the surrounding sentence reads better with a natural phrase than the target title.

```markdown
Full details are in the [Beacon API reference](xref:beacon.api-reference).
```

### 4. Verify the link resolves

At render time, `<xref:uid>` is replaced by an `<a href="…">title</a>` element; `<a href="xref:uid">text</a>` keeps its original text and has its `href` rewritten. Broken xrefs surface as broken-link entries on the build report after running `build`.

---

## Verify

- Run `dotnet run` and hover the link on the source page — the status bar should show the target's real URL, not the literal `xref:<uid>`.
- For the self-closing form, confirm the anchor text equals the target's `title`.
- For the Markdown-link form, confirm the anchor text is your custom text and the `href` points to the target's canonical URL.
- Run `dotnet run -- build <baseUrl> output` and confirm the build report reports zero broken links for the pages you linked.

## Related

- Reference: [Front matter keys](/reference/front-matter/keys) — the `uid` key.
- Reference: [Response processing](/reference/extension-points/response-processing) — rewriter execution order.
- Background: [Cross-reference resolution](/explanation/routing/cross-references)
