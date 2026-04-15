---
title: "Self-host behind Nginx or IIS"
description: "Serve the generated `output/` directory from Nginx or IIS with pretty-URL rewrites and the generated `404.html` as the fallback."
uid: how-to.deployment.self-host
order: 204040
sectionLabel: Publishing & Deployment
tags: [deployment, self-host, nginx, iis]
---

Use this page when you have an `output/` directory produced by `dotnet run -- build` and need to serve it from a server you control — a VPS running Nginx or a Windows host running IIS. When a managed static host is an option, see [Deploy to GitHub Pages](xref:how-to.deployment.github-pages) first; this page is for when that route is unavailable.

## Assumptions

- You have a built `output/` directory (see [Build a static site](xref:how-to.deployment.static-build)) and can copy it onto the target server.
- You have root or administrator access to install a config file and reload the web server.
- The site will serve from the domain root. Sub-path deployments (`https://host/docs/`) require building with `dotnet run -- build /docs` — see [Host under a sub-path (base URL)](xref:how-to.deployment.base-url).
- You are comfortable editing one `nginx.conf` server block or one `web.config` file.

---

## Steps

### 1. Upload `output/` to the web root

Copy the full contents of `output/` to the directory the web server will serve — `/var/www/pennington/` for Nginx or the IIS site's **Physical path** for IIS. Keep the `_content/` and `_spa-data/` folders intact; fingerprinted assets and island payloads live under those underscore-prefixed paths and must ship verbatim.

### 2. Install the server config

Drop the snippet below into your server's config location: Nginx reads its `server` block from `/etc/nginx/sites-enabled/` or `conf.d/`; IIS reads `web.config` from the site root alongside `index.html`. Reload after writing — `nginx -s reload` for Nginx, `iisreset` or an app-pool recycle for IIS.

```nginx:path
examples/SubPathDeployableExample/nginx.conf
```

```xml:path
examples/SubPathDeployableExample/web.config
```

### 3. Serve directory indexes for trailing-slash URLs

Pennington emits every content page as `<slug>/index.html` and every internal link with a trailing slash, so the server must resolve `/guides/first-page/` by serving `/guides/first-page/index.html`. Nginx handles this with `try_files $uri $uri/ /404.html` and `index index.html` — both are already in the snippet above. IIS needs `<defaultDocument>` naming `index.html` plus a rewrite rule that 301-redirects `/guides/first-page` to `/guides/first-page/` so the default-document rule can fire. Without either piece, visitors see a raw directory listing or a 404 on canonical URLs.

### 4. Wire `404.html` as the miss fallback

During `build`, `OutputGenerationService` materializes a real `404.html` at the root of `output/` by rendering the path identified by `NotFoundGeneratorPath`. The web server's only job is to return that file with a 404 status on misses. Nginx does this with `try_files … /404.html;` and `error_page 404 /404.html;`; IIS uses `<httpErrors errorMode="Custom" existingResponse="Replace">` with `<error statusCode="404" path="/404.html" responseMode="File" />`. Both snippets already include this wiring.

```csharp:xmldocid
F:Pennington.Generation.OutputGenerationService.NotFoundGeneratorPath
```

### 5. Fix MIME types and cache headers for fingerprinted assets

Nginx's default `mime.types` usually covers everything Pennington emits, but IIS ships without entries for `.webmanifest` and on some Windows SKUs `.woff2`, so the `web.config` above registers them explicitly. Both snippets also mark `/_content/` fingerprinted assets as `public, immutable` with a one-year expiry — that cache contract is the reason `_content/` paths include content hashes, so preserve it even if you trim the rest of the snippet. The sitemap and `llms.txt` are top-level files; the Nginx snippet sets `default_type` for them, and IIS's built-in `.xml` and `.txt` MIME entries cover them with no extra config.

---

## Verify

- Reload the server, then `curl -I https://<host>/` returns `200 OK` with `content-type: text/html; charset=utf-8` and the landing page renders in a browser.
- `curl -I https://<host>/guides/first-page/` returns 200; dropping the trailing slash still resolves (301 → 200 on IIS, 200 directly on Nginx via `try_files $uri/`).
- `curl -I https://<host>/definitely-not-a-page` returns `404 Not Found` and the body is the generated `404.html`, not the server's default error page.

## Related

- Recipe: [Build a static site](xref:how-to.deployment.static-build) — what `build [baseUrl] [outputDirectory]` produces before you copy `output/` onto the server.
- Recipe: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url) — how `BaseUrlHtmlRewriter` handles a `/docs/` prefix when your Nginx or IIS site does not own the domain root.
- Reference: [CLI and build arguments](xref:reference.host.cli) — the `build [baseUrl] [outputDirectory]` surface that produces the `output/` directory this page serves.
- Background: TODO — add link to the "Unified dev-and-build path" explanation page once published, since it motivates why `404.html` is generated as a real HTTP response rather than a static template.
