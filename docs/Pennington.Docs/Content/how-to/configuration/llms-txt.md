---
title: "Generate an llms.txt"
description: "Expose a stripped-markdown /llms.txt index so LLM crawlers and agents can ingest your site without scraping HTML."
uid: how-to.configuration.llms-txt
order: 202060
sectionLabel: Configuration
tags: [configuration, llms-txt, discovery, front-matter]
---

When you have a working Pennington site and want a machine-readable surface at `/llms.txt` plus per-page stripped-markdown sidecars, this guide shows you how. `AddDocSite` users get llms.txt wired automatically and control the output through front matter. Bare-`AddPennington` users call `AddLlmsTxt(...)` explicitly and choose their own `ContentSelector`. If you do not have a site yet, start with [Your first Pennington site](xref:tutorials.getting-started.first-page).

## Assumptions

- You have a working Pennington site (see [Your first Pennington site](xref:tutorials.getting-started.first-page) if not)
- You know which host extension you are using — `AddDocSite` vs bare `AddPennington` — and why (see [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning))
- The pages you want indexed are reachable over HTTP in dev mode (llms.txt generation fetches each rendered page through the running host)

To see a working DocSite setup with one opted-out page, refer to `Content/main/llms-hidden.md` in `examples/DocSiteKitchenSinkExample`.

---

## Steps

### 1. Decide: DocSite front matter, or bare `AddLlmsTxt`?

`AddDocSite` already calls `AddLlmsTxt` internally and defaults `ContentSelector` to `#main-content`, so on a DocSite host you control per-page inclusion through front matter (step 2) and optionally override the selector through `DocSiteOptions.LlmsTxtContentSelector`. On a bare `AddPennington` host nothing is wired — you must call `AddLlmsTxt(...)` yourself and choose your own `ContentSelector` (step 3).

### 2. (DocSite) Opt a page out with `llms: false`

Every non-draft page is included in the index by default (`Llms = true`). Setting `llms: false` in a page's front matter causes `LlmsTxtService` to skip it when assembling `/llms.txt` and its sidecar markdown. The page still renders, appears in the sidebar, and participates in search unless you also set `search: false`.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md
```

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Llms
```

To use a custom `ContentSelector` (different article wrapper or a non-DocSite layout), set `DocSiteOptions.LlmsTxtContentSelector` — it defaults to `#main-content` but is overridable without leaving DocSite. See [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) for cases that do require bare `AddPennington`.

### 3. (Bare Pennington) Enable `LlmsTxtOptions` with `AddLlmsTxt`

On a bare host nothing is wired until you call `penn.AddLlmsTxt(...)`; the options surface covers `OutputDirectory` (where per-page stripped-markdown sidecars land, defaults to `_llms`), `GenerateFullFile`, and `ContentSelector` (CSS selector that scopes HTML-to-markdown extraction; null means the whole `<body>`).

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.LlmsTxtConfiguration.Configure(Pennington.LlmsTxt.LlmsTxtOptions)
```

```csharp:xmldocid
T:Pennington.LlmsTxt.LlmsTxtOptions
```

### 4. (Optional) Turn on `GenerateFullFile` for a concatenated snapshot

`GenerateFullFile = true` emits `/llms-full.txt`, the same per-page markdown concatenated into one file — useful for one-shot ingest by agents that cannot follow per-page links. The default is `false` because the full file can be large; enable it only when you know the consumer needs it.

```csharp:xmldocid
P:Pennington.LlmsTxt.LlmsTxtOptions.GenerateFullFile
```

---

## Verify

- Run `dotnet run` and fetch `/llms.txt` — expect one line per indexed page, and no line for any page you set `llms: false` on
- Fetch one per-page sidecar (for example, `/_llms/<page>.md`) — expect stripped markdown scoped to your `ContentSelector`, with navigation chrome gone
- If you set `GenerateFullFile = true`, fetch `/llms-full.txt` — expect every sidecar concatenated in one response

## Related

- Reference: [`LlmsTxtOptions` and auxiliary options](xref:reference.options.auxiliary-options)
- How-to: [Configure search indexing](xref:how-to.configuration.search)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
