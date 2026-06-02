# BeyondRemoteContentExample

Bare `AddPennington` host whose content is sourced **entirely from a remote API** —
the [GitHub Releases API](https://docs.github.com/en/rest/releases/releases) — rather
than from the file system. It is the remote twin of `ExtensibilityLabExample`'s
disk-backed `ReleaseNotesContentService`, and the worked example behind
`how-to/content-services/source-from-a-remote-api.md`.

Like `DocSiteSharedCorpusExample`, it legitimately has **no local `Content/`** — every
page comes from the API (or the offline fixture).

## What it demonstrates

- **Typed `HttpClient`** — `GitHubReleasesClient` is registered with
  `AddHttpClient<GitHubReleasesClient>` and sets the mandatory `User-Agent` header and a
  request timeout.
- **One fetch, cached across every pass** — `GitHubReleasesContentService` holds the
  result in an `AsyncLazy<ImmutableList<ReleaseEntry>>`, so discovery, the TOC,
  cross-references, and the rendering endpoint share a single HTTP call per build.
- **Render an API markdown body yourself** — the release `body` is markdown. The
  `MapGet` endpoint runs it through `IContentRenderer` and wraps the result in
  `<article>` (the configured `SiteProjection.ContentSelector`), so the build's
  self-fetch indexes each release at heading level for search and llms.txt.
- **Build-time failure behavior** — when the API is offline, slow, or rate-limited,
  `GitHubReleasesClient` falls back to `fixtures/github-releases.json` so the build
  still succeeds. The fixture also makes `dotnet test` / offline builds deterministic.
- **`EndpointSource` discovery** — each `/releases/{version}/` route is discovered as
  an `EndpointSource` and served by the catch-all `MapGet`. Because it serves real HTML,
  it is included in `sitemap.xml` like any other page.

## Run it

```bash
dotnet run --project examples/BeyondRemoteContentExample
# then open /releases/
```

```bash
dotnet run --project examples/BeyondRemoteContentExample -- build output
# writes output/releases/ and output/search/, snapshotting the API at build time
```

The published site is a point-in-time snapshot. To keep it current, schedule a nightly
rebuild — see the "Keep the published data fresh" section of the how-to.
