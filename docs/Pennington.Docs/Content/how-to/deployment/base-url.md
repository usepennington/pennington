---
title: "Host under a sub-path (base URL)"
description: "Serve a Pennington site from a non-root URL by passing `[baseUrl]` to the build and letting `BaseUrlHtmlRewriter` prefix every internal href, src, and action."
uid: how-to.deployment.base-url
order: 204050
sectionLabel: Publishing & Deployment
tags: [deployment, base-url, rewriter, sub-path]
---

When a site works at root (`/`) locally but the target host — a GitHub Pages project site, a reverse-proxied sub-app, or an Azure Front Door path — serves it under a sub-path, one extra argument to `build` covers the difference. There is no per-link refactor and no separate build mode: the same `RunOrBuildAsync` call handles both cases, with `BaseUrlHtmlRewriter` prefixing every root-relative href, src, and action on the way out.

## Assumptions

- A working Pennington site that builds locally with `dotnet run -- build` (see <xref:how-to.deployment.static-build> if not).
- The sub-path the host will serve from — for example `/docs` for `https://example.com/docs/` or `/<repo>` for a GitHub Pages project site.
- Internal links authored as root-relative (`/guides/first-page/`) — the rewriter keys off the leading `/`.
- The host is already configured to serve `output/` at that sub-path (see <xref:how-to.deployment.github-pages> or <xref:how-to.deployment.self-host>).

For a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample). The nested `/guides/first-page/` route is deliberate: it makes sub-path rewriting observable on a deep link.

---

## Steps

<Steps>
<Step StepNumber="1">

**Pass the base URL to `build`**

`OutputOptions.FromArgs` accepts the sub-path as either a positional token or a named flag; the named flag form survives reordering in CI scripts more reliably. Include the leading slash and omit the trailing slash — the rewriter normalizes either way.

```text
# positional — base URL first, output directory second
dotnet run -- build /docs

# named flag (preferred for CI)
dotnet run -- build --base-url=/docs --output=dist
```

See <xref:reference.host.cli> for the full argument grammar parsed by `OutputOptions.FromArgs`.

</Step>
<Step StepNumber="2">

**Know what the rewriter prefixes**

`BaseUrlHtmlRewriter` runs at `Order => 30` in the `IHtmlResponseRewriter` chain — after xref resolution (10) and locale prefixing (20) — so every upstream transform hands it root-relative paths. It prefixes any `href`, `src`, or `action` attribute whose value starts with `/` (but not `//`, which is protocol-relative) and stamps `data-base-url` on `<body>` for client-side code that needs to reproduce the prefix on dynamically built URLs. See <xref:reference.api.i-response-processor> for the full rewriter surface.

</Step>
<Step StepNumber="3">

**Use root-relative links in your content**

Because the rewriter only matches the leading `/`, protocol-relative URLs (`//cdn.example.com/x.js`) and absolute URLs (`https://…`) pass through untouched, while hash (`#section`) and page-relative links (`./neighbor/`) are ignored. The nested `/guides/first-page/` link in the example's landing page is the smallest case that makes the prefix visible — copy its shape for internal cross-links.

```markdown:path
examples/SubPathDeployableExample/Content/index.md
```

</Step>
<Step StepNumber="4">

**Read `data-base-url` from client-side code**

When an island, Blazor component, or hand-rolled script builds URLs at runtime, read the prefix from `document.body.dataset.baseUrl` rather than hard-coding `/docs`. This keeps a single build portable across hosts — the same `output/` works under `/docs` in staging and `/` in preview with no code change, only a different `--base-url`.

```javascript
const base = document.body.dataset.baseUrl ?? "";
const href = `${base}/guides/first-page/`;
```

</Step>
</Steps>

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
