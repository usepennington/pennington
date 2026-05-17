# EventMicrositeExample

A small conference microsite that exercises both `AddDataFile<T>` and
`AddTaxonomy<TFrontMatter, TKey>` on one bare `AddPennington` host.

## What this teaches

- **`AddDataFile<T>`** — `data/sponsors.yml` and `data/schedule.yml` register as
  typed singletons that hot-reload when the file changes. Razor pages consume
  them through `IDataFiles.Get<T>(name)`.
- **`AddTaxonomy<TFrontMatter, TKey>`** — two taxonomy axes against the same
  `TalkFrontMatter`:
  - `/topic/` — single-valued projection from the `topic:` front-matter key.
  - `/tag/` — multi-valued projection from the standard `tags:` array.
- **Markdown content as taxonomy source** — talks live as one markdown file per
  talk under `Content/talks/`. The taxonomy walker parses each one as
  `TalkFrontMatter` and groups them.
- **`MapTaxonomy<TFrontMatter, TKey>`** — one call mounts every endpoint for
  every `AddTaxonomy` registration of the matching type pair.

## Routes

- `/` — landing page (sponsors strip + schedule preview, both from YAML).
- `/schedule/` — full schedule grid driven by `data/schedule.yml`.
- `/topic/` and `/topic/{slug}/` — taxonomy index + per-topic page.
- `/tag/` and `/tag/{slug}/` — taxonomy index + per-tag page.
- `/talks/{slug}/` — individual talk pages from the markdown source.

## Try it

```bash
dotnet run --project examples/EventMicrositeExample
```

Edit `data/sponsors.yml` while the host is running — the next request to `/`
shows the new sponsor list without an app restart. Same for `data/schedule.yml`
and any markdown file under `Content/talks/`.

## Docs pages

- `how-to/content-services/data-files.md`
- `how-to/content-services/taxonomy.md`
