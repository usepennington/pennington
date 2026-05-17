---
title: "Make the site discoverable to LLM crawlers"
description: "Expose a stripped-markdown /llms.txt index plus per-page sidecars so LLM crawlers and agents can ingest your site without scraping HTML."
uid: how-to.feeds.llms-txt
order: 207010
sectionLabel: "Feeds & Indexes"
tags: [configuration, llms-txt, discovery, front-matter]
---

Expose a `/llms.txt` index plus per-page stripped-markdown sidecars so LLM crawlers and agents can ingest the site without scraping HTML. On `AddDocSite` hosts the wiring is automatic; on bare `AddPennington` hosts a single `AddLlmsTxt(...)` call enables it.

## Before you begin
- A working Pennington site (see <xref:tutorials.getting-started.first-page> if not).
- An `AddDocSite` host (LLM wiring is automatic) or a bare `AddPennington` host (needs an explicit `AddLlmsTxt(...)` call — see the "Bare Pennington" section below). The choice rationale is covered in <xref:explanation.positioning.docsite-positioning>.

For a working DocSite setup with one opted-out page, see `Content/main/llms-hidden.md` in `examples/DocSiteKitchenSinkExample`.

---

## Options

### (DocSite) Opt a page out with `llms: false`

Every non-draft page is included in the index by default (`Llms = true`). Setting `llms: false` in a page's front matter causes `LlmsTxtService` to skip it when assembling `/llms.txt` and its sidecar markdown. The page still renders, appears in the sidebar, and participates in search unless `search: false` is also set.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md
```

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Llms
```

For a custom `ContentSelector` (different article wrapper or a non-DocSite layout), set `DocSiteOptions.LlmsTxtContentSelector`. It defaults to `#main-content` and is overridable without leaving DocSite. See [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning) for cases that do require bare `AddPennington`.

### Split content per-fragment with `humans-only` / `robots-only`

For finer control than page-level opt-out, two paired classes mark a fragment as intended for one audience. Both ship in the MonorailCSS base styles — no registration needed.

- `humans-only` — visible in the browser, stripped from the llms.txt extraction.
- `robots-only` — hidden in the browser via `display: none`, kept in the llms.txt extraction.

```razor
<div class="humans-only">
  <InteractiveTour />
</div>

<div class="robots-only">
  <p>Full method signature: <code>Task&lt;Result&gt; ProcessAsync(Options options, CancellationToken ct = default)</code>.</p>
</div>
```

The classes work anywhere in the rendered page — markdown bodies, Razor components, auto-generated reference pages.

### (Bare Pennington) Enable `LlmsTxtOptions` with `AddLlmsTxt`

On a bare host, call `penn.AddLlmsTxt(...)` once. The options surface (`OutputDirectory`, `GenerateFullFile`, `ContentSelector`) is documented at <xref:reference.api.llms-txt-options).

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.LlmsTxtConfiguration.Configure(Pennington.LlmsTxt.LlmsTxtOptions)
```

Set `GenerateFullFile = true` to also emit `/llms-full.txt` — every sidecar concatenated into one file, useful for one-shot ingest by agents that cannot follow per-page links. Off by default because the file can be large.

---

## Result

`/llms.txt` lists each indexed page as a markdown link grouped by section, and each page gets a stripped-markdown sidecar at `/_llms/<page>.md`. Links are fully qualified when `PenningtonOptions.CanonicalBaseUrl` is set (or `build --base-url https://…` is passed); otherwise they fall back to root-relative `/_llms/...` so an agent that fetched `/llms.txt` can still resolve them against the origin.

A typical excerpt:

```markdown
# Pennington Docs

> Content engine library for .NET.

## Tutorials

- [Your first Pennington site](https://docs.example.com/tutorials/getting-started/first-site): Build a static site from a single markdown file.
- [Add a second locale](https://docs.example.com/tutorials/beyond-basics/add-a-locale): Ship the same content in a second language.

## How-to

- [Switch the body and heading typeface](https://docs.example.com/how-to/configuration/fonts): Self-host woff2, declare preloads, point the family options at the new faces.
```

## Verify

- Run `dotnet run` and fetch `/llms.txt`. Expect a metadata block, a `## Map` section listing per-subtree splits with token estimates, then nav-grouped links — and no line for any page marked `llms: false`
- Fetch one per-page sidecar (for example, `/_llms/<page>.md`). Expect a YAML header with `canonical_url`, `content_hash`, `tokens`, and the body stripped to clean markdown
- With `GenerateFullFile = true`, fetch `/llms-full.txt`. Expect every sidecar concatenated in one response

## Related

- Reference: [`LlmsTxtOptions`](xref:reference.api.llms-txt-options)
- How-to: [Tune what the search box returns](xref:how-to.discovery.search)
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning)
