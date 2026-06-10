---
title: "Self-host behind Nginx or IIS"
description: "Serve the generated `output/` directory from Nginx or IIS with pretty-URL rewrites and the generated `404.html` as the fallback."
uid: how-to.deployment.self-host
order: 4
sectionLabel: "Publishing & Deployment"
tags: [deployment, self-host, nginx, iis]
---

Serve an `output/` directory produced by `dotnet run -- build` from a server you control — a VPS running Nginx or a Windows host running IIS. When a managed static host is an option, [Deploy to GitHub Pages](xref:how-to.deployment.github-pages) is simpler.

## Before you begin
- A built `output/` directory (see [Build a static site](xref:how-to.deployment.static-build)), ready to copy onto the target server.
- Root or administrator access to install a config file and reload the web server.
- The site serves from the domain root. Sub-path deployments (`https://host/docs/`) require building with `dotnet run -- build /docs` — see [Host under a sub-path (base URL)](xref:how-to.deployment.base-url).

---

## Steps

<Steps>
<Step StepNumber="1">

**Upload `output/` to the web root**

Copy the full contents of `output/` to the directory the web server will serve — `/var/www/pennington/output/` for Nginx (the path the snippet's `root` points at) or the IIS site's **Physical path** for IIS. Keep the `_content/` folder intact; fingerprinted static-web-asset bundles (Razor library CSS and JS) live under that underscore-prefixed path and ship verbatim.

</Step>
<Step StepNumber="2">

**Install the server config**

Drop the snippet for your server into its config location and reload. Both snippets cover trailing-slash directory indexes, the generated `404.html` as the miss fallback (served with a real 404 status), and `public, immutable` cache headers on `/_content/` fingerprinted assets. The IIS snippet also declares MIME types for `.webmanifest` and `.woff2`, which IIS does not know by default; Nginx serves those from its global `mime.types` include.

### Nginx

Drop into `/etc/nginx/sites-enabled/` (or `conf.d/`), then `nginx -s reload`.

```nginx:symbol
examples/SubPathDeployableExample/nginx.conf
```

### IIS

Drop `web.config` into the site root alongside `index.html`, then run `iisreset` or recycle the app pool.

```xml:symbol
examples/SubPathDeployableExample/web.config
```

</Step>
</Steps>

---

## Serve under a sub-path

When the site does not own the domain root — it lives at `https://host/docs/` — build with the prefix (`dotnet run -- build --base-url=/docs`, see [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)) so every internal link carries it, then point the server at `output/` under that same path.

For Nginx, mount the directory with `alias` (not `root`) inside a `location` block named for the prefix:

```nginx
location /docs/ {
    alias /var/www/pennington/output/;
    try_files $uri $uri/ =404;
}
```

For IIS, host the site as an application or virtual directory named `docs` and drop the same `web.config` into its physical path — the rewrite and `404.html` rules apply relative to the application root, so no changes are needed.

## Verify

- Reload the server, then `curl -I https://<host>/` returns `200 OK` with `content-type: text/html; charset=utf-8` and the landing page renders in a browser.
- `curl -I https://<host>/guides/first-page/` returns 200; dropping the trailing slash still resolves (301 → 200 on IIS, 200 directly on Nginx via `try_files $uri/`).
- `curl -I https://<host>/definitely-not-a-page` returns `404 Not Found` and the body is the generated `404.html` rather than the server's default error page.

## Related

- Recipe: [Build a static site](xref:how-to.deployment.static-build) — what `build [baseUrl] [outputDirectory]` produces before you copy `output/` onto the server.
- Recipe: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url) — how `BaseUrlHtmlRewriter` handles a `/docs/` prefix when your Nginx or IIS site does not own the domain root.
- Reference: [CLI and build arguments](xref:reference.host.cli) — the `build [baseUrl] [outputDirectory]` surface that produces the `output/` directory this page serves.
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — motivates why `404.html` is generated as a real HTTP response rather than a static template.
