---
title: "Source content from a remote API"
description: "Implement IContentService over a typed HttpClient to turn a remote JSON API — here the GitHub Releases API — into routed pages, navigation, search, and xref targets, with one cached fetch per build, rendered markdown bodies, and a fixture fallback when the API is down."
uid: how-to.content-services.remote-api
order: 2
sectionLabel: "Content Services"
tags: [extensibility, content-service, http, remote, caching]
---

To build pages from a remote HTTP API instead of local files — a release feed, a CMS, a product catalog behind a JSON endpoint — implement `IContentService` over a typed `HttpClient`. This guide is the remote counterpart to <xref:how-to.content-services.custom-content-service>: that page covers the discovery, TOC, and cross-reference work every content service shares. This one adds the four things a *network* source needs:

- awaiting HTTP in `DiscoverAsync`,
- caching one fetch across every pipeline pass,
- rendering markdown bodies that arrive over the wire,
- and surviving a slow or unreachable API at build time.

The recipe references `examples/BeyondRemoteContentExample`, which turns the [GitHub Releases API](https://docs.github.com/en/rest/releases/releases) into `/releases/{version}/` pages.

## Before you begin

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site>).
- The discovery shape from <xref:how-to.content-services.custom-content-service> — this guide assumes you know how `DiscoverAsync`, `GetContentTocEntriesAsync`, and `GetCrossReferencesAsync` fit together, and focuses on what changes when the source is remote.
- An API reachable over HTTP that returns JSON. The example uses an unauthenticated public endpoint; for an authenticated API, add the token to the typed client's default headers.

## Fetch the data with a typed `HttpClient`

Register a typed `HttpClient` with `AddHttpClient<T>`. Set a `User-Agent` — the GitHub API answers `403` without one — and a `Timeout`, so a stalled API cannot hang the build:

```csharp
builder.Services.AddHttpClient<GitHubReleasesClient>(client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Pennington-Remote-Content-Example");
    client.Timeout = TimeSpan.FromSeconds(10);
});
```

The client itself is a thin wrapper that fetches and deserializes. `GetFromJsonAsync` with a snake-case naming policy maps GitHub's `tag_name`/`html_url`/`published_at` onto a PascalCase record:

```csharp:symbol
examples/BeyondRemoteContentExample/GitHubReleasesClient.cs > GitHubReleasesClient.GetReleasesAsync
```

The `try`/`catch` is the build-time failure boundary — covered under [Handle a slow or unreachable API](#handle-a-slow-or-unreachable-api-at-build-time) below.

## Cache the fetch across every pass

`DiscoverAsync`, the TOC pass, the cross-reference pass, and the rendering endpoint each need the data. Fetching in each one would hit the API four-plus times per build. Cache the result in an `AsyncLazy<T>` (from `Pennington.Infrastructure`) created once in the constructor, and `await` it everywhere:

```csharp
private readonly AsyncLazy<ImmutableList<ReleaseEntry>> _entriesLazy;

public GitHubReleasesContentService(GitHubReleasesClient client)
    => _entriesLazy = new AsyncLazy<ImmutableList<ReleaseEntry>>(() => LoadAsync(client));
```

`AsyncLazy<T>` runs its factory once on first access and replays the same task to every later caller; a faulted fetch is evicted so the next access retries. `DiscoverAsync` awaits it like every other pass, pairing each route with `EndpointSource` so the build crawler fetches the URL through the endpoint below:

```csharp:symbol
examples/BeyondRemoteContentExample/GitHubReleasesContentService.cs > GitHubReleasesContentService.DiscoverAsync
```

Because this service reads no local files, there is nothing to file-watch — register it as a process-lifetime singleton (see [Register the service](#register-the-service)). A service backed by files that change during a dev session needs the file-watched lifetimes described in <xref:how-to.feeds.custom-feed>, or it serves stale data.

## Render the API's markdown body yourself

GitHub returns each release's notes as markdown. `EndpointSource` routes are *not* run through Markdig automatically — the framework hands the route to your endpoint and renders nothing — so call `IContentRenderer` yourself. Build a `ParsedItem` from the markdown and render it; the result's `Content.Html` is the same output a markdown file would produce:

```csharp:symbol
examples/BeyondRemoteContentExample/ReleasePages.cs > ReleasePages.RenderDetailAsync
```

Wrap that HTML in the element named by `SiteProjection.ContentSelector` (here `<article>`, set in `Program.cs`). At build time the projection self-fetches every TOC-listed route through the live pipeline and splits the selected element into heading sections — so an `EndpointSource` page rendered this way **is indexed at heading level for search and llms.txt**, exactly like a markdown page. Without the selector match, the page chrome leaks into the index instead of the release body.

> [!NOTE]
> Because each release page serves real canonical HTML at a stable URL, it **is** included in `sitemap.xml` — `EndpointSource` routes are crawled like any other page. (Only `RedirectSource` and `LlmsOnlySource` routes, which have no canonical HTML, are left out.) See <xref:how-to.feeds.sitemap>.

## Register the service

Register the concrete type, then forward `IContentService` to the same instance so the endpoint and the pipeline share one cache:

```csharp
builder.Services.AddSingleton<GitHubReleasesContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<GitHubReleasesContentService>());
```

A singleton holding a typed `HttpClient` is normally discouraged — the pooled handler stops rotating, so long-lived processes can pin stale DNS. Here it is fine: the client is used for a single fetch at startup and never again; the *cache*, not the client, is what lives for the process. For a long-running host that re-fetches periodically, inject `IHttpClientFactory` and create a client per fetch instead.

## Handle a slow or unreachable API at build time

A build that fetches from the network inherits the network's failure modes: the API can be down, rate-limited, or slow. The `Timeout` on the typed client bounds the slow case. For the rest, `GetReleasesAsync` **fails open** — on `HttpRequestException`, a timeout, or malformed JSON it logs a warning and falls back to a committed fixture (`fixtures/github-releases.json`), so an offline or rate-limited build still produces a complete site. The fixture also makes the build deterministic in CI.

To **fail closed** instead — stop the build when the data is unavailable rather than ship a snapshot that may be stale or empty — rethrow from the `catch` and let it propagate out of `DiscoverAsync`. Choose per site: a marketing build might prefer last-known-good data; a compliance build might prefer to fail loudly.

## Keep the published data fresh

> [!WARNING]
> A static build is a point-in-time snapshot. The published site shows the releases that existed *when you built it* and will not change until you build again — a new release on GitHub does not appear on its own.

For content that updates on its own schedule, rebuild on a schedule. A nightly GitHub Actions job rebuilds and redeploys so the live site never drifts more than a day behind:

```yaml
on:
  schedule:
    - cron: "0 6 * * *"   # 06:00 UTC daily
  workflow_dispatch:       # plus a manual trigger

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet run --project examples/BeyondRemoteContentExample -- build output
      # ...then deploy output/ as usual
```

Wire the deploy step the same way a push build does — see <xref:how-to.deployment.github-pages>.

## Verify

- Run `dotnet run --project examples/BeyondRemoteContentExample` and open `/releases/`. The index lists every release and each `/releases/{version}/` renders its notes as formatted HTML (headings, lists, links), not raw markdown.
- Run `dotnet run --project examples/BeyondRemoteContentExample -- build output`. Confirm `output/releases/` has one folder per release, the rendered markdown carries `<h2>` headings, `output/search/` indexes those heading texts (proving the `EndpointSource` bodies reach search via the self-fetch), and `output/sitemap.xml` lists every `/releases/{version}/` URL.
- Disconnect from the network and rebuild. The build still succeeds, serving the fixture snapshot.

## Related

- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service) — the base `IContentService` shape this guide builds on.
- How-to: [Publish a custom feed from a content service](xref:how-to.feeds.custom-feed) — the DI lifetimes for a *file-backed* service, and the stale-cache trap.
- How-to: [Publish a sitemap](xref:how-to.feeds.sitemap) — what the sitemap includes and excludes.
- Background: [Why ContentSource is a union](xref:explanation.core.content-source) — what `EndpointSource` means and why its pages are still listed in the sitemap.
