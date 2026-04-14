---
title: "Generate an llms.txt"
description: "Expose a stripped-markdown /llms.txt index so LLM crawlers and agents can ingest your site without scraping HTML."
uid: how-to.configuration.llms-txt
order: 60
sectionLabel: Configuration
tags: [configuration, llms-txt, discovery, front-matter]
---

> **In this page.** _Paraphrase TOC "Covers": enable `LlmsTxtOptions`, pick an `OutputDirectory`, turn on `GenerateFullFile` for a concatenated snapshot, and drop pages out of the index with `llms: false`. One or two sentences — no preamble about what llms.txt is._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": the policy / training implications of publishing an llms.txt, and generating an MCP server from the same content (out of scope). One sentence, link-free or linking only if a neighbouring explanation page lands later._

## When to use this

_Two to three sentences. The reader already has a Pennington site rendering HTML and now wants a machine-readable surface at `/llms.txt` (plus per-page stripped-markdown sidecars). Name the two realistic arrival paths: `AddDocSite` users get llms.txt wired automatically and steer the output through front matter; bare-`AddPennington` users call `AddLlmsTxt(...)` explicitly and set the content selector themselves. If the reader has no site yet, point back to [_Your first Pennington site_](/tutorials/getting-started-core/first-page)._

## Assumptions

_Keep to 3 bullets. The non-obvious one is the DocSite vs. bare split, which decides which step the reader follows._

- You have a working Pennington site (see [_Your first Pennington site_](/tutorials/getting-started-core/first-page) if not)
- You know which host extension you are using — `AddDocSite` vs bare `AddPennington` — and why ([_When is DocSite the right starting point?_](/explanation/core/docsite-positioning))
- The pages you want indexed are reachable over HTTP in dev mode (llms.txt generation fetches each rendered page through the running host)

To copy a working DocSite setup with one opted-out page, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — specifically `Content/main/llms-hidden.md`, which sets `llms: false`. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four steps. Step 1 orients the reader on their host shape. Steps 2–3 are the recipe (one for DocSite via front matter, one for bare Pennington via `AddLlmsTxt`). Step 4 is the optional `GenerateFullFile` toggle. Keep each step under two sentences of prose plus at most one fence._

### 1. Decide: DocSite front matter, or bare `AddLlmsTxt`?

_One or two sentences. `AddDocSite` already calls `AddLlmsTxt` internally and pins `ContentSelector = "#main-content"` alongside the search index, so on DocSite you only touch per-page front matter (step 2). On a bare `AddPennington` host nothing is wired by default — you must call `AddLlmsTxt(...)` yourself and choose your own `ContentSelector` (step 3)._

### 2. (DocSite) Opt a page out with `llms: false`

_One or two sentences. The default on every `IFrontMatter` is `Llms = true`, so every non-draft page lands in the index; setting `llms: false` makes `LlmsTxtService` skip that page when assembling `/llms.txt` and its sidecar markdown. The page still renders, still appears in the sidebar, and still participates in search unless you also set `search: false`._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md
```

_Show the backing property for completeness:_

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Llms
```

_Callout: if you need a custom `ContentSelector` (different article wrapper, a non-DocSite layout), `DocSiteOptions` does not expose that knob — drop to bare `AddPennington` and follow step 3. Link: [_When is DocSite the right starting point?_](/explanation/core/docsite-positioning)._

### 3. (Bare Pennington) Enable `LlmsTxtOptions` with `AddLlmsTxt`

_One sentence. On a bare host nothing is wired until you call `penn.AddLlmsTxt(...)`; the options surface is small — `OutputDirectory` (where the per-page stripped-markdown sidecars land under the publish output, defaults to `_llms`), `GenerateFullFile`, and `ContentSelector` (CSS selector that scopes HTML→markdown extraction; null means "whole `<body>`")._

TODO — no example currently shows a bare `AddPennington` host calling `AddLlmsTxt` directly. Until one lands, use this inline shape and link the options class for verification:

```csharp
builder.Services.AddPennington(penn =>
{
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });

    penn.AddLlmsTxt(opts =>
    {
        opts.OutputDirectory = "_llms";     // default
        opts.ContentSelector = "article";   // adjust to your layout
        opts.GenerateFullFile = false;      // flip to true for step 4
    });
});
```

_Show the backing options record so the reader can cross-check every property:_

```csharp:xmldocid
T:Pennington.LlmsTxt.LlmsTxtOptions
```

### 4. (Optional) Turn on `GenerateFullFile` for a concatenated snapshot

_One or two sentences. `GenerateFullFile = true` emits `/llms-full.txt`, the same per-page markdown concatenated into one file — useful for one-shot ingest by agents that cannot follow per-page links. Default is `false` because the full file can be large; toggle only when you know the consumer wants it._

```csharp:xmldocid
P:Pennington.LlmsTxt.LlmsTxtOptions.GenerateFullFile
```

---

## Verify

_Three terse bullets — one per output surface. The reader should confirm each without reading anything else._

- Run `dotnet run` and fetch `/llms.txt` — expect one line per indexed page, and no line for any page you set `llms: false` on
- Fetch one per-page sidecar (e.g. `/_llms/<page>.md`) — expect stripped markdown scoped to your `ContentSelector`, with navigation chrome gone
- If you set `GenerateFullFile = true`, fetch `/llms-full.txt` — expect every sidecar concatenated in one response

## Related

_Two to four cross-quadrant links. Reference for the exhaustive option catalogue, the sibling search how-to (same `#main-content` pinning story), and the Explanation page that explains why DocSite pins selectors. Do not link to the next how-to in this section — generated automatically._

- Reference: [_`LlmsTxtOptions` and auxiliary options_](/reference/options/auxiliary-options)
- How-to: [_Configure search indexing_](/how-to/configuration/search)
- Background: [_When is DocSite the right starting point?_](/explanation/core/docsite-positioning)
