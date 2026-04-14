---
title: "Adapt the deploy workflow for other hosts"
description: "Port the canonical GitHub Pages recipe to Azure Static Web Apps, Cloudflare Pages, or Netlify by swapping four shared values and dropping in one host-specific config file."
uid: how-to.deployment.adapt-for-other-hosts
order: 204030
sectionLabel: Publishing & Deployment
tags: [deployment, azure-static-web-apps, cloudflare-pages, netlify]
---

> **In this page.** The shared shape every host needs (build command, publish directory, .NET SDK pin, base URL) plus a per-host table of deltas for Azure Static Web Apps, Cloudflare Pages, and Netlify — with inline `staticwebapp.config.json` and `netlify.toml` snippets so you can copy the routing and fallback rules verbatim.
>
> **Not in this page.** The shared CI steps themselves — those live on [_Deploy to GitHub Pages_](xref:how-to.deployment.github-pages), which is the canonical recipe this page diffs against. Self-hosting behind Nginx or IIS has its own how-to at [_Self-host behind Nginx or IIS_](xref:how-to.deployment.self-host).

## When to use this

_Two sentences. Trigger: you have read the GitHub Pages how-to and understand the `dotnet run -- build [baseUrl]` → `output/` → artifact pipeline, and now want to ship the same site to Azure Static Web Apps, Cloudflare Pages, or Netlify instead. If you have not followed the GitHub Pages recipe first, go back — this page only describes the deltas and will not make sense in isolation._

## Assumptions

_Bulleted list. Four bullets max. Keep it tight; if this grows, readers should be on a tutorial._

- You have already worked through [_Deploy to GitHub Pages_](xref:how-to.deployment.github-pages) and the canonical workflow builds cleanly against your project.
- You have a deploy target account created (SWA resource, Cloudflare Pages project, or Netlify site) and the repo connected.
- Your site either serves at the host's domain root (`baseUrl = "/"`) or you know the exact sub-path you need to pass as the first positional argument to `build`.
- You are comfortable editing one host config file per target — the snippets below are complete, not starting points.

To copy a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample). The `.github/workflows/deploy.yml`, `staticwebapp.config.json`, and `netlify.toml` siblings are the teaching surface for this page; do not walk through the whole example.

---

## Steps

### 1. Lock in the four shared values

_Two sentences. Every host configuration has the same four knobs: the **build command** (`dotnet run --project <your-project> -- build "$BASE_URL"`), the **publish directory** (always `output/` — this is the default from `OutputOptions.OutputDirectory`), the **.NET SDK pin** (`11.0.x`, matching `setup-dotnet@v4` in the GitHub Pages workflow), and the **base URL** (empty `/` for apex/root domains, `/<path>` for sub-path hosting). Write these four down before you open any host dashboard — every per-host file below is just those four values expressed in that host's syntax._

```csharp:xmldocid
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

### 2. Pick your host and copy the per-host deltas

_One to two sentences. The table below is the authoritative diff between the GitHub Pages workflow and each supported host — if a cell is blank, nothing changes from the canonical recipe. Read the row for your host, then jump to the matching step for the host-specific config file (Azure in step 3, Netlify in step 4, Cloudflare in step 5)._

| Concern | GitHub Pages (canonical) | Azure Static Web Apps | Cloudflare Pages | Netlify |
|---|---|---|---|---|
| Config file | `.github/workflows/deploy.yml` | `staticwebapp.config.json` at repo root + SWA's own build action | Pages dashboard or `wrangler.toml` | `netlify.toml` at repo root |
| Build command | `dotnet run --project … -- build "$BASE_URL"` | same (invoked via `Azure/static-web-apps-deploy@v1` `app_build_command`) | same (set in dashboard → **Build command**) | same (declared in `[build] command`) |
| Publish directory | `output` (via `upload-pages-artifact@v3`) | `output_location: "output"` on the SWA action | **Build output directory:** `output` | `publish = "output"` |
| .NET SDK pin | `actions/setup-dotnet@v4` with `11.0.x` | add `actions/setup-dotnet@v4` before the SWA action | dashboard env: `DOTNET_VERSION = 11.0.x` (Cloudflare autodetects from there) | `[build.environment] DOTNET_VERSION = "11.0.x"` |
| Base URL strategy | derived from `${{ github.event.repository.name }}` | pass explicitly — SWA serves at apex by default, so usually `""` | pass explicitly — Cloudflare serves at apex, usually `""` | `$BASE_URL` env var with `/` default; override in dashboard per site |
| SPA / deep-link fallback | `.nojekyll` marker + `404.html` | `navigationFallback.rewrite: "/404.html"` (see step 3) | Cloudflare auto-serves `404.html` from build output — no extra config | `[[redirects]]` with `status = 404 → /404.html` (see step 4) |
| Cache headers for `/_content/*` | GitHub Pages default (short TTL) | `routes[]` entry, `Cache-Control: public, max-age=31536000, immutable` | `_headers` file in `output/` (same directive) | `[[headers]] for = "/_content/*"` (same directive) |
| `.nojekyll` needed? | yes | no (SWA does not run Jekyll) | no | no |

### 3. Azure Static Web Apps — drop in `staticwebapp.config.json`

_Two sentences. Commit the JSON below at the repo root; SWA reads it during deploy and applies routes, MIME overrides, nav fallback, and 404 handling. Then in your SWA workflow (`.github/workflows/azure-static-web-apps-<id>.yml`, generated by the Azure portal) make sure `app_build_command` is `dotnet run --project <your-project> -- build` and `output_location` is `output` — everything else from the canonical GitHub Pages workflow still applies._

```json:path
examples/SubPathDeployableExample/staticwebapp.config.json
```

### 4. Netlify — drop in `netlify.toml`

_Two sentences. Commit the TOML below at the repo root; Netlify autodetects it and no dashboard build-setting edits are needed beyond linking the repo. The `BASE_URL` environment variable defaults to `/`; override it in **Site configuration → Environment variables** if you need a sub-path, and the `[[redirects]]` block with `status = 404` wires deep-link misses back to the generated `output/404.html`._

```toml:path
examples/SubPathDeployableExample/netlify.toml
```

### 5. Cloudflare Pages — configure in dashboard (no config file needed)

_Two sentences. Cloudflare Pages does not ship a first-party config file equivalent to SWA or Netlify, so set everything in the project's dashboard under **Settings → Builds & deployments**: **Build command** = `dotnet run --project <your-project> -- build`, **Build output directory** = `output`, **Environment variables** `DOTNET_VERSION=11.0.x` and `BASE_URL=/` (or your sub-path). If you need custom cache headers for `/_content/*`, drop a `_headers` file into `wwwroot/` so it ships as part of `output/` — the format is identical to the directive in the Netlify and Azure snippets above._

### 6. Pass the right `baseUrl` for the host's URL shape

_Two sentences. GitHub Pages defaults to a sub-path; Azure Static Web Apps, Cloudflare Pages, and Netlify default to the apex, so the build argument you pass is usually different. For apex deploys pass nothing (or `/`); for a sub-path pass `/<path>` and let `BaseUrlHtmlRewriter` prefix every internal href, src, and action on the way out — detailed handling lives in [_Host under a sub-path (base URL)_](xref:how-to.deployment.base-url)._

```csharp:xmldocid
T:Pennington.Infrastructure.BaseUrlHtmlRewriter
```

---

## Verify

- Trigger a deploy on the target host; the build log shows `setup-dotnet` (or equivalent) picking up `11.0.x`, `dotnet run -- build` exiting zero, and the host uploading `output/` as the publish directory.
- Open the deployed URL — the landing page loads, nested links resolve, and view-source shows the expected `<body data-base-url="...">` (empty or `/<path>/` depending on the host).
- Visit a non-existent path like `/does-not-exist/` — the response body is the generated `output/404.html`, not the host's default 404 shell.

## Related

- Recipe: [_Deploy to GitHub Pages_](xref:how-to.deployment.github-pages) — the canonical workflow this page diffs against.
- Recipe: [_Self-host behind Nginx or IIS_](xref:how-to.deployment.self-host) — for hosts where you own the web server config instead of a managed platform.
- Recipe: [_Host under a sub-path (base URL)_](xref:how-to.deployment.base-url) — how `BaseUrlHtmlRewriter` prefixes internal URLs when the host serves under `/<path>/`.
- Reference: [_CLI and build arguments_](xref:reference.host.cli) — the `build [baseUrl] [outputDirectory]` surface every host command above invokes.
