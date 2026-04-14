---
title: "Self-host behind Nginx or IIS"
description: "Serve the generated `output/` directory from Nginx or IIS with pretty-URL rewrites and the generated `404.html` as the fallback."
uid: how-to.deployment.self-host
order: 40
sectionLabel: Publishing & Deployment
tags: [deployment, self-host, nginx, iis]
---

> **In this page.** _Paraphrase TOC "Covers": serving `output/` as static files from Nginx or IIS, wiring the directory-index / default-document rules Pennington's trailing-slash URLs expect, and routing misses to the generated `404.html`. Two sentences max, pitched at a reader who already has a built `output/` directory on the server._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": running the live Pennington ASP.NET host (the `dotnet run` origin) behind Nginx or IIS as a reverse proxy is a separate topic — link out to [Build a static site](/how-to/deployment/static-build) as the shape this page assumes and note that the reverse-proxy recipe is not yet documented (TODO: add once covered)._

## When to use this

_Two sentences. Trigger: you already produce an `output/` directory via `dotnet run -- build` and need to serve it from a server you control — typically a VPS running Nginx or a Windows host running IIS — rather than a managed static platform. If you are still deciding between this and a managed host, start on [Deploy to GitHub Pages](/how-to/deployment/github-pages); this page is for when that route is not available._

## Assumptions

_Four bullets. Keep prerequisites minimal — if the list grows, the page is a tutorial._

- You have a built `output/` directory (see [Build a static site](/how-to/deployment/static-build) if not) and can copy it onto the target server.
- You have root / administrator access to install a config file and reload the web server.
- The site will serve from the domain root; sub-path deployments (`https://host/docs/`) still work but require building with `dotnet run -- build /docs` — see [Host under a sub-path (base URL)](/how-to/deployment/base-url).
- You are comfortable editing one `nginx.conf` server block or one `web.config` file — this page does not wrap either in a deployment tool.

To copy a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample). The `nginx.conf` and `web.config` siblings of the csproj are the teaching artefacts; do not walk through the whole example.

---

## Steps

_Five steps, Nginx and IIS snippets shown side by side per step rather than forking into two pages. Every step should apply to both servers; where the mechanics diverge, the Nginx fence comes first, the IIS fence second, and the prose names both._

### 1. Upload `output/` to the web root

_Two sentences. Copy the full contents of `output/` to the directory the web server will serve — `/var/www/pennington/output` in the Nginx example, the IIS site's **Physical path** in the web.config example. Keep the `_content/` and `_spa-data/` folders intact — Pennington's fingerprinted assets and island payloads live under those underscore-prefixed paths and need to ship verbatim._

### 2. Install the server config

_One to two sentences. Drop the snippet below next to (or inside) the web root: Nginx reads its `server` block from `/etc/nginx/sites-enabled/…` or `conf.d/…`, IIS reads `web.config` from the site root alongside `index.html`. Reload after writing — `nginx -s reload` for Nginx, `iisreset` or an app-pool recycle for IIS._

```nginx:path
examples/SubPathDeployableExample/nginx.conf
```

```xml:path
examples/SubPathDeployableExample/web.config
```

### 3. Serve directory indexes for trailing-slash URLs

_Two to three sentences. Pennington emits every content page as `<slug>/index.html` and every internal link with a trailing slash, so the server must resolve `/guides/first-page/` by serving `/guides/first-page/index.html`. Nginx handles this with `try_files $uri $uri/ /404.html` plus `index index.html` (already in the snippet above); IIS needs both `<defaultDocument>` to name `index.html` and the rewrite rule that 301-redirects `/guides/first-page` to `/guides/first-page/` so the default-document rule can fire. Without either piece the reader sees a raw directory listing or a 404 on canonical URLs._

### 4. Wire `404.html` as the miss fallback

_Two to three sentences. `OutputGenerationService` fetches its sentinel `NotFoundGeneratorPath` during `build` to materialize a real `404.html` at the root of `output/`, so the only job left to the web server is to hand that file back with a 404 status on misses. Nginx does this with `try_files … /404.html;` plus `error_page 404 /404.html;`; IIS uses `<httpErrors errorMode="Custom" existingResponse="Replace">` with `<error statusCode="404" path="/404.html" responseMode="File" />`. Both snippets above already include the wiring — verify by hitting a bogus URL and confirming the 404 body is the styled Pennington page, not the server's default._

```csharp:xmldocid
F:Pennington.Generation.OutputGenerationService.NotFoundGeneratorPath
```

### 5. Fix MIME types and cache headers for fingerprinted assets

_Two to three sentences. Nginx's default `mime.types` usually covers everything Pennington emits, but IIS ships without entries for `.webmanifest` and (on some Windows SKUs) `.woff2`, so the `web.config` above registers them explicitly. Both snippets also mark `/\_content/` fingerprinted assets as `public, immutable` with a one-year expiry — that cache contract is the whole reason `_content/` paths include content hashes, so preserve it even if you trim the rest of the snippet. Sitemap and llms.txt are served as top-level files; the Nginx snippet sets `default_type` for them, IIS's built-in `.xml` and `.txt` MIME entries cover them with no extra config._

---

## Verify

- Reload the server, then `curl -I https://<host>/` returns `200 OK` with `content-type: text/html; charset=utf-8` and the landing page renders in a browser.
- `curl -I https://<host>/guides/first-page/` returns 200; dropping the trailing slash still resolves (301 → 200 on IIS, 200 directly on Nginx via `try_files $uri/`).
- `curl -I https://<host>/definitely-not-a-page` returns `404 Not Found` and the body is the generated `404.html`, not the server's default error page.

## Related

- Recipe: [Build a static site](/how-to/deployment/static-build) — what `build [baseUrl] [outputDirectory]` produces before you copy `output/` onto the server.
- Recipe: [Host under a sub-path (base URL)](/how-to/deployment/base-url) — how `BaseUrlHtmlRewriter` handles a `/docs/` prefix when your Nginx or IIS site does not own the domain root.
- Reference: [CLI and build arguments](/reference/host/cli) — the `build [baseUrl] [outputDirectory]` surface that produces the `output/` directory this page serves.
- Background: TODO — add link to the "Unified dev-and-build path" explanation page once published, since it motivates why `404.html` is generated as a real HTTP response rather than a static template.
