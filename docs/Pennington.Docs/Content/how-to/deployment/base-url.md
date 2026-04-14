---
title: "Host under a sub-path (base URL)"
description: "Serve a Pennington site from a non-root URL by passing `[baseUrl]` to the build and letting `BaseUrlHtmlRewriter` prefix every internal href, src, and action."
uid: how-to.deployment.base-url
order: 50
sectionLabel: Publishing & Deployment
tags: [deployment, base-url, rewriter, sub-path]
---

> **In this page.** _One sentence paraphrased from `docs-toc.md` "Covers": passing `[baseUrl]` to the `build` command and how `BaseUrlHtmlRewriter` prefixes root-relative anchors, assets, and form actions on the way out of the response pipeline._
>
> **Not in this page.** _One sentence paraphrased from `docs-toc.md` "Does not cover": base-URL handling for custom client-side routers that bypass Pennington's SPA island system — wire those up yourself by reading `<body data-base-url>` in the client bundle._

## When to use this

_Two sentences. Frame the arrival state: the reader has a site that builds and serves fine at root (`/`) but the target host (GitHub Pages project site, reverse-proxied sub-app, Azure Front Door behind `/docs/`) serves it under a sub-path. Point out that the fix is a build-time arg plus a rewriter — there is no per-link refactor and no separate "base-aware" build, just the same `RunOrBuildAsync` with one extra token._

## Assumptions

_Three or four bullets. Keep it tight — anything longer signals the reader is still on a tutorial._

- You have a working Pennington site that builds locally with `dotnet run -- build` (see [_Build a static site_](xref:how-to.deployment.static-build) if not).
- You know the sub-path the host will serve you under — for example `/docs` for `https://example.com/docs/` or `/<repo>` for a GitHub Pages project site.
- Internal links are authored as root-relative (`/guides/first-page/`) — the rewriter keys off the leading `/`.
- Your host is already configured to serve `output/` at that sub-path (see [_Deploy to GitHub Pages_](xref:how-to.deployment.github-pages) or [_Self-host behind Nginx or IIS_](xref:how-to.deployment.self-host)).

To copy a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample). The nested `/guides/first-page/` route is deliberate: it makes sub-path rewriting observable on a deep link. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four steps. Imperative verbs. Do not describe a second rewriter or a parallel "base-aware" renderer — there is one path: pass the arg, the rewriter fires, view-source confirms._

### 1. Pass the base URL to `build`

_Two sentences. `OutputOptions.FromArgs` accepts the sub-path as either a positional token (`build /docs`) or a named flag (`build --base-url=/docs`); named flags survive reordering in CI scripts better. Leave the leading slash on, and do not include a trailing slash — the rewriter trims it either way._

```text
# positional — base URL first, output directory second
dotnet run -- build /docs

# named flag (preferred for CI)
dotnet run -- build --base-url=/docs --output=dist
```

_The parser is the single source of truth for which shapes are accepted:_

```csharp:xmldocid
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

### 2. Know what the rewriter prefixes

_Two sentences. `BaseUrlHtmlRewriter` runs at `Order => 30` in the `IHtmlResponseRewriter` chain — after xref resolution (10) and locale prefixing (20) — so every upstream transform hands it logical root-relative paths. It prefixes any `href`, `src`, or `action` attribute whose value starts with `/` (but not `//`, which is protocol-relative) and stamps `data-base-url` on `<body>` for client-side code that needs to reproduce the prefix on dynamically built links._

```csharp:xmldocid
T:Pennington.Infrastructure.BaseUrlHtmlRewriter
```

### 3. Use root-relative links in your content

_Two sentences. Because the rewriter only matches the leading `/`, protocol-relative URLs (`//cdn.example.com/x.js`) and absolute URLs (`https://…`) pass through untouched, while hash (`#section`) and page-relative links (`./neighbor/`) are ignored. The nested `/guides/first-page/` link in the example's landing page is the smallest case that makes the prefix visible — copy its shape for internal cross-links._

```markdown:path
examples/SubPathDeployableExample/Content/index.md
```

### 4. Read `data-base-url` from client-side code

_Two sentences. If an island, Blazor component, or hand-rolled script builds URLs at runtime, read the prefix from `document.body.dataset.baseUrl` rather than hard-coding `/docs`. This keeps a single build portable across hosts — the same `output/` works under `/docs` in staging and `/` in preview with no code change, just a different `--base-url`._

```javascript
const base = document.body.dataset.baseUrl ?? "";
const href = `${base}/guides/first-page/`;
```

---

## Verify

_Terse. Three bullets; no rereading required._

- Run `dotnet run -- build --base-url=/docs` and open `output/index.html` — every internal `href`, `src`, and `action` now starts with `/docs/`, and `<body>` carries `data-base-url="/docs"`.
- Serve `output/` under the same sub-path (e.g. `npx http-server output -p 5000` behind a reverse proxy at `/docs/`) — deep links like `/docs/guides/first-page/` resolve and their in-page links stay under the prefix.
- Re-run with no `--base-url` — the generated HTML reverts to root-relative paths with no `data-base-url` attribute, confirming the rewriter short-circuits via `ShouldApply` when the prefix is empty or `/`.

## Related

_Two to four cross-quadrant links. Point at the reference surface for the CLI and at the explanation for why the rewriter is ordered last. Do not link to the next how-to in this section — nav is auto-generated from `order:`._

- Reference: [_CLI and build arguments_](xref:reference.host.cli) — the `build [baseUrl] [outputDirectory]` surface this page drives.
- Reference: TODO — `BaseUrlHtmlRewriter` API page if one exists under `/reference/infrastructure/` (grep `docs-toc.md` before publishing; if absent, drop this bullet).
- Background: [_The response-processing pipeline_](xref:explanation.core.response-processing) — why base-URL rewriting runs at `Order => 30`, after xref and locale rewriters.
- Background: [_Dev mode and build mode share one code path_](xref:explanation.core.dev-vs-build) — why the same rewriter runs identically in `dotnet run` and `dotnet run -- build`.
