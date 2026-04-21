---
title: "Make the site discoverable to LLM crawlers"
description: "Expose a stripped-markdown /llms.txt index plus per-page sidecars so LLM crawlers and agents can ingest your site without scraping HTML."
uid: how-to.configuration.llms-txt
order: 202060
sectionLabel: Configuration
tags: [configuration, llms-txt, discovery, front-matter]
---

When LLM crawlers and agents should ingest the site without scraping HTML, the `/llms.txt` index plus per-page stripped-markdown sidecars give them a direct surface. On `AddDocSite` hosts the wiring is automatic and output is controlled through front matter; on bare `AddPennington` hosts `AddLlmsTxt(...)` is called explicitly with a chosen `ContentSelector`. If no site exists yet, start with [Your first Pennington site](xref:tutorials.getting-started.first-page).

## Assumptions

- A working Pennington site (see [Your first Pennington site](xref:tutorials.getting-started.first-page) if not)
- The chosen host extension — `AddDocSite` vs bare `AddPennington` — and the reason for that choice (see [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning))
- Pages that should be indexed are reachable over HTTP in dev mode (llms.txt generation fetches each rendered page through the running host)

For a working DocSite setup with one opted-out page, refer to `Content/main/llms-hidden.md` in `examples/DocSiteKitchenSinkExample`.

---

## Options

### Decide: DocSite front matter, or bare `AddLlmsTxt`?

`AddDocSite` already calls `AddLlmsTxt` internally and defaults `ContentSelector` to `#main-content`. On a DocSite host, per-page inclusion is controlled through front matter (below), with an optional selector override through `DocSiteOptions.LlmsTxtContentSelector`. On a bare `AddPennington` host nothing is wired — `AddLlmsTxt(...)` needs an explicit call with a chosen `ContentSelector`.

### (DocSite) Opt a page out with `llms: false`

Every non-draft page is included in the index by default (`Llms = true`). Setting `llms: false` in a page's front matter causes `LlmsTxtService` to skip it when assembling `/llms.txt` and its sidecar markdown. The page still renders, appears in the sidebar, and participates in search unless `search: false` is also set.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md
```

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Llms
```

For a custom `ContentSelector` (different article wrapper or a non-DocSite layout), set `DocSiteOptions.LlmsTxtContentSelector`. It defaults to `#main-content` and is overridable without leaving DocSite. See [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) for cases that do require bare `AddPennington`.

### Split content per-fragment with `humans-only` / `robots-only`

For finer control than page-level opt-out, two paired classes mark a fragment as intended for one audience or the other. Both ship as part of the MonorailCSS base styles, so no registration is needed.

- `humans-only` — visible in the browser, stripped from the llms.txt extraction. Reach for it when a widget, interactive demo, or layout flourish carries no information an LLM needs.
- `robots-only` — hidden in the browser via `display: none`, kept in the llms.txt extraction. Reach for it when an LLM needs context the human reader already has visually (a full signature next to a compact hover card, prose that mirrors a diagram, etc.).

```razor
<div class="humans-only">
  <InteractiveTour />
</div>

<div class="robots-only">
  <p>Full method signature: <code>Task&lt;Result&gt; ProcessAsync(Options options, CancellationToken ct = default)</code>.</p>
</div>
```

The classes work anywhere inside the `ContentSelector` — markdown bodies, Razor components, auto-generated reference pages.

### (Bare Pennington) Enable `LlmsTxtOptions` with `AddLlmsTxt`

On a bare host nothing is wired until `penn.AddLlmsTxt(...)` is called. The options surface covers `OutputDirectory` (where per-page stripped-markdown sidecars land, defaults to `_llms`), `GenerateFullFile`, and `ContentSelector` (CSS selector that scopes HTML-to-markdown extraction; null means the whole `<body>`).

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.LlmsTxtConfiguration.Configure(Pennington.LlmsTxt.LlmsTxtOptions)
```

```csharp:xmldocid
T:Pennington.LlmsTxt.LlmsTxtOptions
```

### (Optional) Turn on `GenerateFullFile` for a concatenated snapshot

`GenerateFullFile = true` emits `/llms-full.txt`, the same per-page markdown concatenated into one file — useful for one-shot ingest by agents that cannot follow per-page links. The default is `false` because the full file can be large; enable it when a known consumer needs it.

```csharp:xmldocid
P:Pennington.LlmsTxt.LlmsTxtOptions.GenerateFullFile
```

---

## Result

`/llms.txt` lists each indexed page as a markdown link grouped by section, and each page gets a stripped-markdown sidecar at `/_llms/<page>.md`. A typical excerpt:

```text
# Pennington Docs

> Content engine library for .NET.

## Tutorials

- [Your first Pennington site](https://docs.example.com/tutorials/getting-started/first-site): Build a static site from a single markdown file.
- [Add a second locale](https://docs.example.com/tutorials/beyond-basics/add-a-locale): Ship the same content in a second language.

## How-to

- [Switch the body and heading typeface](https://docs.example.com/how-to/configuration/fonts): Self-host woff2, declare preloads, point the family options at the new faces.
```

## Verify

- Run `dotnet run` and fetch `/llms.txt`. Expect one line per indexed page, and no line for any page marked `llms: false`
- Fetch one per-page sidecar (for example, `/_llms/<page>.md`). Expect stripped markdown scoped to the `ContentSelector`, with navigation chrome gone
- With `GenerateFullFile = true`, fetch `/llms-full.txt`. Expect every sidecar concatenated in one response

## Related

- Reference: [`LlmsTxtOptions`](xref:reference.api.llms-txt-options)
- How-to: [Tune what the search box returns](xref:how-to.configuration.search)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
