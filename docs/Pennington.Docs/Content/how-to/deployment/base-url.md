---
title: Host under a sub-path (base URL)
description: Pass a base URL to the build command and let BaseUrlHtmlRewriter prefix every anchor, asset, and script at response time.
section: deployment
order: 70
tags: []
uid: how-to.deployment.base-url
isDraft: true
search: false
llms: false
---

> **In this page.** Passing `[baseUrl]` to the build command and how `BaseUrlHtmlRewriter` rewrites anchors, assets, and scripts.
>
> **Not in this page.** Client-side-router base handling outside the built-in SPA island system.

## When to use this

- You have a working Pennington site that currently serves at `/` and you need to host it under a sub-path (for example `https://example.com/docs/` or a GitHub Pages project URL like `https://user.github.io/repo/`).
- You are writing markdown with root-relative links (`/guides/intro/`, `/images/logo.svg`) and want those to survive the prefix without rewriting every source file.

## Assumptions

- You have a working Pennington site whose `Program.cs` ends with `await app.RunOrBuildAsync(args)` (or the `RunDocSiteAsync` / `RunBlogSiteAsync` wrappers).
- You have already produced a static build at the default root once, so you know the build crawler works (see [Build a static site](/how-to/deployment/static-build)).
- Internal links in your markdown use either relative paths (which `MarkdownLinkResolver` handles) or root-relative paths (which `BaseUrlHtmlRewriter` handles).

To copy a working setup, see [`examples/BeaconDocsExample`](https://github.com/phil-scott-78/Pennington/tree/main/examples/BeaconDocsExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Pick the sub-path you will host under

- The value must match the first path segment(s) the host serves from: `/docs/`, `/preview/`, `/repo/`.
- Write it leading-slashed; a trailing slash is tolerated (`BaseUrlHtmlRewriter` trims it).
- Use `/` (or omit the argument) when you are hosting at the host root — the rewriter short-circuits and produces byte-identical output to a no-prefix build.

### 2. Pass the base URL as the second positional build argument

`OutputOptions.FromArgs` parses `args[0]="build"`, `args[1]=BaseUrl`, `args[2]=OutputDirectory`. Put the base URL between the verb and the output directory:

```shell
dotnet run --project src/MySite -- build /docs ./dist
```

- The `build` token must be `args[0]` or the host falls back to `app.RunAsync()` and the base URL is ignored.
- `args[1]` defaults to `"/"` when omitted; `args[2]` defaults to `"output"`.
- Quote the value if your shell would otherwise swallow the leading slash.

### 3. Let `BaseUrlHtmlRewriter` prefix root-relative attributes

Registered at `Order = 30` (last in the HTML-rewrite pass, after xref at 10 and locale-link at 20). For every response whose `BaseUrl` is non-empty and not `/`, it walks the parsed document and prefixes:

- Every `[href]`, `[src]`, and `[action]` value that starts with a single `/` (not `//`).
- `//cdn.example.com/x.js` and absolute `https://…` URLs pass through untouched.
- Pure-fragment (`#anchor`), `mailto:`, `tel:`, and relative hrefs are untouched — they do not start with `/`.

### 4. Rely on `<body data-base-url="…">` for client-side URL construction

The rewriter also stamps `data-base-url` on `<body>` so scripts that build URLs at runtime can mirror the server prefix:

```javascript
let baseUrl = document.body.getAttribute('data-base-url') || '';
fetch(baseUrl + '/_spa-data/index.json');
```

- The built-in SPA navigation (`/_spa-data`) and search-widget scripts read this attribute; custom scripts that fetch JSON endpoints should do the same.
- The value has no trailing slash, matching how the rewriter trims on construction.

### 5. Verify the prefix matches your canonical base URL setting

- `DocSiteOptions.CanonicalBaseUrl` / `BlogSiteOptions.CanonicalBaseUrl` is independent of `[baseUrl]` — it feeds the sitemap, RSS, and JSON-LD, not the HTML rewriter.
- Set both when hosting under a sub-path on a real domain: `CanonicalBaseUrl = "https://example.com/docs"` and build with `-- build /docs ./dist`.
- A mismatch produces a site that renders correctly but emits absolute sitemap/RSS URLs that resolve to 404.

---

## Verify

- Run `dotnet run --project src/MySite -- build /docs ./dist`, open `dist/index.html`, and confirm internal links render as `href="/docs/…"` instead of `href="/…"`.
- Search the same file for `data-base-url="/docs"` on the `<body>` tag.
- Run the same command with `-- build / ./dist` (or omit the base URL) and confirm the output has no `/docs/` prefix and no `data-base-url` attribute — dev serve behaviour is unchanged.

## Related

- Reference: [OutputOptions and CLI arguments](/reference/generation/output-options/)
- Reference: [Response rewriters and processor pipeline](/reference/infrastructure/response-processors/)
- Background: [How URL rewriting is layered (xref -> locale -> base URL)](/explanation/url-rewriting-pipeline/)
