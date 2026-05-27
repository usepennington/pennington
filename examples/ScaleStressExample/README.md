# ScaleStressExample

A bare Pennington host that serves a generated corpus of **5000 markdown files** through a single `MarkdownContentService`. The intent is to exercise content discovery, parsing, and routing at a volume that authored examples don't reach.

## How it works

- `Content/` is gitignored. On first launch, `CorpusGenerator.EnsureAsync` checks the folder, and if it has fewer than 5000 `doc-NNNN.md` files it trains a tiny word-bigram Markov chain on the `examples/_shared/Bramble/Content` corpus and writes the missing files before the web host starts.
- Generation is seeded (`Seed = 1138`), so output is the same on every machine until you bump the constant or change the corpus.
- The host wires one source: `AddMarkdownContent<DocFrontMatter>` rooted at `Content/`, mapped to `/`.
- `GET /` lists every discovered document via `IContentService.GetIndexableEntriesAsync()` (cheap — the service caches metadata).
- `GET /{*path}` walks `DiscoverAsync()` on the way through and renders the matching file. Deliberately the same naive shape as `GettingStartedMinimalSiteExample` — at 5000 files the per-request iteration is observable, which is the point.

## Generated file shape

Each `doc-NNNN.md` has:

- YAML front matter with `title`, `description`, `uid` (`stress.doc-NNNN`), `order`, and a small tag list.
- A short intro paragraph, then 3–5 `##` sections, each with 1–3 paragraphs.
- Roughly half the sections embed one of **four fixed code samples** (C#, Python, JavaScript, Bash). Same handful in every file — they are syntactically valid placeholders, not meaningful code.

## Run it

```bash
dotnet run --project examples/ScaleStressExample
```

First run takes a few seconds while the corpus is generated. Subsequent runs start immediately.

To regenerate the corpus, delete `Content/` and re-launch.

## Concepts

- Single `MarkdownContentService<DocFrontMatter>` over a flat 5000-file directory.
- Pre-startup file generation guarded by a count check.
- No styling, no DocSite — minimal HTML wrapper around `IContentRenderer` output.
