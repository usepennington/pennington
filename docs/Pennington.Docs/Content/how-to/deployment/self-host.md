---
title: "Self-host behind Nginx or IIS"
description: "Serve the generated output/ directory from Nginx or IIS with default-extension rules and the generated 404.html."
section: "deployment"
order: 60
tags: []
uid: how-to.deployment.self-host
isDraft: true
search: false
llms: false
---

> **In this page.** Serving the `output/` directory as static files, setting default-extension rules, and handling 404s with the generated `404.html`.
>
> **Not in this page.** Running the live Pennington app as an origin (a valid but separate topic).

## When to use this

- You have an `output/` directory produced by `dotnet run -- build` and need to host it from an existing Nginx or IIS server you already administer.
- Pick this page over a static-host recipe (S3, Netlify, Pages) when you want to terminate TLS, serve other apps, or run inside a private network on the same box.

## Assumptions

- You ran the static build and the `output/` directory contains `index.html`, nested `*/index.html` pages, `404.html`, static assets, and (if enabled) `sitemap.xml`, `/search-index-{locale}.json`, `/llms.txt`.
- You have root/Administrator access to a server that already has Nginx (>= 1.18) or IIS (>= 10) installed and serving something.
- You built with the correct `BaseUrl` — the site lives at `/` unless you passed a sub-path to `dotnet run -- build /sub/ ./output`.
- You are shipping the build artifact directly, not proxying back to the ASP.NET host. For an origin-server setup, see the separate how-to.

To copy a working build output, see [`examples/MinimalExample`](https://github.com/Phil-Scott-DEV/Pennington/tree/main/examples/MinimalExample) and run `dotnet run --project examples/MinimalExample -- build` first.

---

## Steps

### 1. Copy the `output/` directory to the server

Deploy the artifact to the web root you intend to serve. The directory contains nested `index.html` files (every page is emitted as `path/index.html`) plus `404.html` at the root.

```shell
rsync -az --delete ./output/ deploy@host:/var/www/pennington/
```

### 2. Configure Nginx with directory-index fallback and the 404 page

Point `root` at the copied directory. `try_files` must try the URI, then `$uri/index.html` (so `/guides/intro/` resolves to `/guides/intro/index.html`), then the `@notfound` named location which serves `/404.html` with a 404 status.

```nginx
server {
    listen 80;
    server_name example.com;
    root /var/www/pennington;

    index index.html;

    location / {
        try_files $uri $uri/ $uri/index.html @notfound;
    }

    location @notfound {
        return 404 /404.html;
    }

    error_page 404 /404.html;
    location = /404.html {
        internal;
    }
}
```

Reload with `sudo nginx -t && sudo systemctl reload nginx`.

### 3. Add MIME types for Pennington artifacts Nginx may not know

Nginx's default `mime.types` covers `.html`, `.css`, `.js`, `.json`, `.svg`. Confirm `.xml` (sitemap) and `.txt` (`llms.txt`) return `application/xml` and `text/plain`; if your distro's `mime.types` is trimmed, add overrides:

```nginx
types {
    application/xml    xml;
    text/plain         txt;
    application/json   json;
}
```

### 4. Configure IIS with a `web.config` placed at the site root

IIS reads `web.config` from the served folder. Drop this file next to `index.html` in `output/` (or copy it into the deploy target). It enables directory default documents, routes 404s to the generated page, and adds the MIME types IIS blocks by default.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <defaultDocument>
      <files>
        <clear />
        <add value="index.html" />
      </files>
    </defaultDocument>
    <httpErrors errorMode="Custom" existingResponse="Replace">
      <remove statusCode="404" />
      <error statusCode="404" path="/404.html" responseMode="File" />
    </httpErrors>
    <staticContent>
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <remove fileExtension=".webmanifest" />
      <mimeMap fileExtension=".webmanifest" mimeType="application/manifest+json" />
    </staticContent>
  </system.webServer>
</configuration>
```

### 5. (Optional) Redirect trailing-slashless URLs

Pennington emits directory-style URLs (`/guides/intro/`). Requests to `/guides/intro` (no slash) work via `try_files $uri/` on Nginx and IIS default documents on IIS, but you can force a canonical redirect for SEO:

```nginx
# Nginx: 301 /foo -> /foo/ when a matching directory exists
location ~ ^([^.]+[^/])$ {
    try_files $uri @addslash;
}
location @addslash {
    return 301 $uri/;
}
```

```xml
<!-- IIS: requires URL Rewrite module -->
<rewrite>
  <rules>
    <rule name="AddTrailingSlash" stopProcessing="true">
      <match url="(.*[^/])$" />
      <conditions>
        <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
        <add input="{REQUEST_URI}" pattern="\." negate="true" />
      </conditions>
      <action type="Redirect" url="{R:1}/" redirectType="Permanent" />
    </rule>
  </rules>
</rewrite>
```

### 6. (Optional) Serve a sub-path build

If you built with `dotnet run -- build /pennington/ ./output`, mount the directory under that prefix so internal links — rewritten by `BaseUrlHtmlRewriter` — resolve. On Nginx use `location /pennington/ { alias /var/www/pennington/; ... }`; on IIS create a virtual application at `/pennington` whose physical path is the output folder.

---

## Verify

- `curl -sI https://example.com/` returns `200` and `Content-Type: text/html`.
- `curl -sI https://example.com/guides/intro/` returns `200` (directory-index fallback works).
- `curl -sI https://example.com/does-not-exist/` returns `404` and the body matches `output/404.html` (`diff <(curl -s https://example.com/does-not-exist/) output/404.html`).

## Related

- Reference: [OutputOptions and CLI arguments](/reference/generation/output-options/)
- Background: [Why dev-serve and build share one HTTP path](/explanation/architecture/unified-build-pipeline/)
