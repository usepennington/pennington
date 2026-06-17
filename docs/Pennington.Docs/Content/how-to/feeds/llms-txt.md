---
title: "Make the site discoverable to LLM crawlers"
description: "Expose a stripped-markdown /llms.txt index plus per-page sidecars so LLM crawlers and agents can ingest your site without scraping HTML."
uid: how-to.feeds.llms-txt
order: 1
sectionLabel: "Feeds & Indexes"
tags: [configuration, llms-txt, discovery, front-matter]
---

Expose a `/llms.txt` index plus per-page stripped-markdown sidecars so LLM crawlers and agents can read the site without scraping HTML. On `AddDocSite` hosts the wiring is automatic; on bare `AddPennington` hosts a single `AddLlmsTxt(...)` call enables it.

## Before you begin
- A working Pennington site (see <xref:tutorials.getting-started.first-page> if not).
- An `AddDocSite` host (LLM wiring is automatic) or a bare `AddPennington` host (needs an explicit `AddLlmsTxt(...)` call — see the "Bare Pennington" section below). The choice rationale is covered in <xref:explanation.positioning.docsite-positioning>.

---

## Options

### (Bare Pennington) Enable `LlmsTxtOptions` with `AddLlmsTxt`

On a bare host nothing is wired until you ask for it: call `penn.AddLlmsTxt(...)` once to turn on the index and sidecars. `AddDocSite` hosts skip this — the wiring is automatic. The options surface (`OutputDirectory`, `GenerateFullFile`) is documented at <xref:reference.api.llms-txt-options>. The chrome-stripping selector lives one layer up at `penn.SiteProjection.ContentSelector` — it is shared with the search index so both channels see the same body element.

```csharp:symbol,bodyonly
examples/ExtensibilityLabExample/LlmsTxtConfiguration.cs > LlmsTxtConfiguration.Configure
```

Set `GenerateFullFile = true` to also emit `/llms-full.txt` — every sidecar concatenated into one file, useful for one-shot ingest by agents that cannot follow per-page links. Off by default because the file can be large.

### Report the documented version with `SiteVersion`

The `/llms.txt` front door stamps a version line so a crawler can tell which release the content describes. By default that is Pennington's own package version, emitted as `penningtonVersion:` — which says what generated the file, not what it documents. When your site documents a specific library or product, set `PenningtonOptions.SiteVersion` to that subject's version; the front door then emits `version:` and drops `penningtonVersion:`.

```csharp
penn.SiteTitle = "Spectre.Console";
penn.SiteVersion = SpectreVersion.FromReferencedAssembly().Display; // "0.57"
```

Resolve the value however suits the site — a constant, a build property, or the informational version of a referenced assembly.

### (DocSite) Opt a page out with `llms: false`

Every non-draft page is included in the index by default (`Llms = true`). Setting `llms: false` in a page's front matter causes `LlmsTxtService` to skip it when assembling `/llms.txt` and its sidecar markdown. The page still renders, appears in the sidebar, and participates in search unless `search: false` is also set. `Content/main/llms-hidden.md` in `examples/DocSiteKitchenSinkExample` is a working opted-out page:

```markdown:symbol
examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md
```

```csharp:symbol
src/Pennington.DocSite/DocSiteFrontMatter.cs > DocSiteFrontMatter.Llms
```

### Point the chrome-stripping selector at a custom wrapper

The selector that picks the body element out of the rendered page — for a different article wrapper or a non-DocSite layout — is `DocSiteOptions.ContentSelector`. It defaults to `#main-content` and is overridable without leaving DocSite. The same selector drives the search index, llms.txt sidecars, and the build-time link audit, so chrome is stripped once. (On a bare host the equivalent knob is `penn.SiteProjection.ContentSelector`, shown above.) See [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning) for cases that do require bare `AddPennington`.

### Split content per-fragment with `humans-only` / `robots-only`

For finer control than page-level opt-out, two paired classes mark a fragment as intended for one audience. Both ship in the [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) base styles — no registration needed.

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

---

## Result

`/llms.txt` lists each indexed page as a markdown link grouped by section, and each page gets a stripped-markdown sidecar at `/_llms/<page>.md`. Links are fully qualified when `PenningtonOptions.CanonicalBaseUrl` is set (or `build --base-url https://…` is passed); otherwise they fall back to root-relative `/_llms/...` so an agent that fetched `/llms.txt` can still resolve them against the origin.

A typical front door — a metadata block, a `## Map` of any subtrees split into their own `{prefix}llms.txt`, then the nav-grouped links:

```markdown
# Pennington Docs

> Content engine library for .NET.

site: https://docs.example.com/
canonical: https://docs.example.com/llms.txt
generated: 2026-06-09 14:02 UTC
penningtonVersion: 0.1.0

## Map

- [Reference](https://docs.example.com/reference/llms.txt) (96 entries, ~120k tokens) — API surface, host extensions, front-matter keys, Markdig extensions, UI components, diagnostics codes.

## Tutorials

- [Your first Pennington site](https://docs.example.com/tutorials/getting-started/first-site): Build a static site from a single markdown file.
- [Add a second locale](https://docs.example.com/tutorials/beyond-basics/add-a-locale): Ship the same content in a second language.

## How-to

- [Switch the body and heading typeface](https://docs.example.com/how-to/configuration/fonts): Self-host woff2, declare preloads, point the family options at the new faces.
```

The `## Map` block appears only when a subtree is declared (a folder's `_meta.yml` carries an `llms:` block, or a content service registers one) — those leaves move to `/reference/llms.txt` and the front door keeps just the see-also line above. A site with no subtrees jumps straight from the metadata block to the nav-grouped links.

## Verify

- Run `dotnet run` and fetch `/llms.txt`. Expect a metadata block, a `## Map` section listing per-subtree splits with token estimates, then nav-grouped links — and no line for any page marked `llms: false`
- Fetch one per-page sidecar (for example, `/_llms/<page>.md`). Expect a YAML header with `canonical_url`, `content_hash`, `tokens`, and the body stripped to clean markdown
- With `GenerateFullFile = true`, fetch `/llms-full.txt`. Expect every sidecar concatenated in one response

## Related

- Reference: [`LlmsTxtOptions`](xref:reference.api.llms-txt-options)
- How-to: [Tune what the search box returns](xref:how-to.discovery.search)
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning)
