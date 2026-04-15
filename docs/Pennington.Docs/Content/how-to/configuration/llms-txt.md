---
title: "Generate an llms.txt"
description: "Expose a stripped-markdown /llms.txt index so LLM crawlers and agents can ingest your site without scraping HTML."
uid: how-to.configuration.llms-txt
order: 202060
sectionLabel: Configuration
tags: [configuration, llms-txt, discovery, front-matter]
---

To add a machine-readable surface at `/llms.txt` plus per-page stripped-markdown sidecars to a working Pennington site, follow this guide. On `AddDocSite` hosts, llms.txt is wired automatically and output is controlled through front matter. On bare `AddPennington` hosts, `AddLlmsTxt(...)` is called explicitly with a chosen `ContentSelector`. If no site exists yet, start with [Your first Pennington site](xref:tutorials.getting-started.first-page).

## Assumptions

- A working Pennington site (see [Your first Pennington site](xref:tutorials.getting-started.first-page) if not)
- The chosen host extension — `AddDocSite` vs bare `AddPennington` — and the reason for that choice (see [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning))
- Pages that should be indexed are reachable over HTTP in dev mode (llms.txt generation fetches each rendered page through the running host)

For a working DocSite setup with one opted-out page, refer to `Content/main/llms-hidden.md` in `examples/DocSiteKitchenSinkExample`.

---

## Steps

<Steps>
<Step StepNumber="1">

**Decide: DocSite front matter, or bare `AddLlmsTxt`?**

`AddDocSite` already calls `AddLlmsTxt` internally and defaults `ContentSelector` to `#main-content`. On a DocSite host, per-page inclusion is controlled through front matter (step 2), with an optional selector override through `DocSiteOptions.LlmsTxtContentSelector`. On a bare `AddPennington` host nothing is wired — `AddLlmsTxt(...)` needs an explicit call with a chosen `ContentSelector` (step 3).

</Step>
<Step StepNumber="2">

**(DocSite) Opt a page out with `llms: false`**

Every non-draft page is included in the index by default (`Llms = true`). Setting `llms: false` in a page's front matter causes `LlmsTxtService` to skip it when assembling `/llms.txt` and its sidecar markdown. The page still renders, appears in the sidebar, and participates in search unless `search: false` is also set.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md
```

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Llms
```

For a custom `ContentSelector` (different article wrapper or a non-DocSite layout), set `DocSiteOptions.LlmsTxtContentSelector`. It defaults to `#main-content` and is overridable without leaving DocSite. See [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) for cases that do require bare `AddPennington`.

</Step>
<Step StepNumber="3">

**(Bare Pennington) Enable `LlmsTxtOptions` with `AddLlmsTxt`**

On a bare host nothing is wired until `penn.AddLlmsTxt(...)` is called. The options surface covers `OutputDirectory` (where per-page stripped-markdown sidecars land, defaults to `_llms`), `GenerateFullFile`, and `ContentSelector` (CSS selector that scopes HTML-to-markdown extraction; null means the whole `<body>`).

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.LlmsTxtConfiguration.Configure(Pennington.LlmsTxt.LlmsTxtOptions)
```

```csharp:xmldocid
T:Pennington.LlmsTxt.LlmsTxtOptions
```

</Step>
<Step StepNumber="4">

**(Optional) Turn on `GenerateFullFile` for a concatenated snapshot**

`GenerateFullFile = true` emits `/llms-full.txt`, the same per-page markdown concatenated into one file — useful for one-shot ingest by agents that cannot follow per-page links. The default is `false` because the full file can be large; enable it when a known consumer needs it.

```csharp:xmldocid
P:Pennington.LlmsTxt.LlmsTxtOptions.GenerateFullFile
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run` and fetch `/llms.txt`. Expect one line per indexed page, and no line for any page marked `llms: false`
- Fetch one per-page sidecar (for example, `/_llms/<page>.md`). Expect stripped markdown scoped to the `ContentSelector`, with navigation chrome gone
- With `GenerateFullFile = true`, fetch `/llms-full.txt`. Expect every sidecar concatenated in one response

## Related

- Reference: [`LlmsTxtOptions` and auxiliary options](xref:reference.options.auxiliary-options)
- How-to: [Configure search indexing](xref:how-to.configuration.search)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
