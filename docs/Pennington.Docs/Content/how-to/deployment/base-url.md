---
title: "Host under a sub-path (base URL)"
description: "Serve a Pennington site from a non-root URL by passing `[baseUrl]` to the build and letting `BaseUrlHtmlRewriter` prefix every internal href, src, and action."
uid: how-to.deployment.base-url
order: 211050
sectionLabel: "Publishing & Deployment"
tags: [deployment, base-url, rewriter, sub-path]
---

To serve under a sub-path, pass it as the first argument to `build`. `BaseUrlHtmlRewriter` prefixes every root-relative `href`, `src`, and `action` on the way out; the same `RunOrBuildAsync` call handles root and sub-path identically.

## Before you begin
- A working Pennington site that builds locally with `dotnet run -- build` (see <xref:how-to.deployment.static-build> if not).
- **The sub-path the host will serve from** — for example `/docs` for `https://example.com/docs/` or `/<repo>` for a GitHub Pages project site.
- Internal links authored as root-relative (`/guides/first-page/`). The rewriter only matches the leading `/`; protocol-relative (`//cdn.example.com/x.js`), absolute (`https://…`), hash (`#section`), and page-relative (`./neighbor/`) links pass through untouched.
- The host is already configured to serve `output/` at that sub-path (see <xref:how-to.deployment.github-pages> or <xref:how-to.deployment.self-host>).

For a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample).

## Build with the prefix

`OutputOptions.FromArgs` accepts the sub-path as a positional token or a named flag; named flags survive CI script reorderings more reliably. Include the leading slash and omit the trailing slash — the rewriter normalizes either way.

```bash
# positional — base URL first, output directory second
dotnet run -- build /docs

# named flag (preferred for CI)
dotnet run -- build --base-url=/docs --output=dist
```

See <xref:reference.host.cli> for the argument grammar.

## Reproduce the prefix from client-side code

When an island, Blazor component, or hand-rolled script builds URLs at runtime, read the prefix from `document.body.dataset.baseUrl` (stamped by the rewriter) instead of hard-coding `/docs`. The same `output/` then runs under `/docs` in staging and `/` in preview with only a different `--base-url`.

```javascript
const base = document.body.dataset.baseUrl ?? "";
const href = `${base}/guides/first-page/`;
```

---

## Verify

- Run `dotnet run -- build --base-url=/docs` and open `output/index.html` — every internal `href`, `src`, and `action` now starts with `/docs/`, and `<body>` carries `data-base-url="/docs"`.
- Serve `output/` under the same sub-path (for example `npx http-server output -p 5000` behind a reverse proxy at `/docs/`) — deep links like `/docs/guides/first-page/` resolve and their in-page links stay under the prefix.
- Re-run with no `--base-url` — the generated HTML reverts to root-relative paths with no `data-base-url` attribute, confirming the rewriter short-circuits when the prefix is empty or `/`.

## Related

- Reference: <xref:reference.host.cli> — the `build [baseUrl] [outputDirectory]` surface this page drives.
- Reference: <xref:reference.api.base-url-html-rewriter> — the rewriter that prefixes every root-relative `href`, `src`, and `action` and stamps `data-base-url` on `<body>`.
- Background: <xref:explanation.core.response-processing> — why base-URL rewriting runs at `Order => 30`, after xref and locale rewriters.
- Background: <xref:explanation.core.dev-vs-build> — why the same rewriter runs identically in `dotnet run` and `dotnet run -- build`.
