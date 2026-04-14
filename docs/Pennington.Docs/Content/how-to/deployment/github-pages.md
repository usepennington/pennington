---
title: "Deploy to GitHub Pages"
description: "Ship a Pennington site to GitHub Pages with a ready-to-copy Actions workflow, base-URL injection, and the `.nojekyll` marker."
uid: how-to.deployment.github-pages
order: 204020
sectionLabel: Publishing & Deployment
tags: [deployment, github-pages, ci, base-url]
---

> **In this page.** A ready-to-copy GitHub Actions workflow using `actions/setup-dotnet@v4`, `actions/upload-pages-artifact@v3`, and `actions/deploy-pages@v4`, plus the `.nojekyll` marker that keeps Pennington's `_content/` folders intact. This is the canonical host recipe that the other deployment how-tos adapt.
>
> **Not in this page.** Custom-domain DNS setup beyond ticking the GitHub Pages "custom domain" checkbox — configure that in the repository's Pages settings once the workflow is green.

## When to use this

_Two sentences. Trigger: you have a working Pennington site committed to a GitHub repo and want Pages to build-and-deploy it on every push to `main`. Do not reach for this page if the site still only runs under `dotnet run` — land on [_Build a static site_](xref:how-to.deployment.static-build) first so you know what `output/` should look like before you automate producing it._

## Assumptions

_Short bulleted list. No more than four bullets — if the list grows, the reader should be back on a tutorial._

- You have a Pennington site that builds locally with `dotnet run --project <your-project> -- build` (see [_Build a static site_](xref:how-to.deployment.static-build) if not).
- The repo is pushed to GitHub and Pages is enabled under **Settings → Pages → Build and deployment → Source: GitHub Actions**.
- The site will be served under a repository sub-path like `https://<user>.github.io/<repo>/` — root-domain deployments are called out in Step 5.
- You are comfortable with GitHub Actions YAML at the "copy, commit, inspect the run log" level.

To copy a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample). The `.github/workflows/deploy.yml`, host-config siblings (`staticwebapp.config.json`, `netlify.toml`, `nginx.conf`, `web.config`), and the `BuildHost` helper are the teaching surface; do not walk through the whole example.

---

## Steps

### 1. Enable GitHub Pages with the Actions source

_One to two sentences. In the repo settings switch **Pages → Build and deployment → Source** to **GitHub Actions** so the deploy workflow below is authorized to publish. Also confirm the three workflow permissions the deploy action needs — `contents: read`, `pages: write`, `id-token: write` — are not blocked at the organization level; the workflow declares them explicitly but an org-wide deny will still win._

### 2. Drop in the canonical workflow

_Two sentences. Commit the YAML below to `.github/workflows/deploy.yml` at the repo root. It pins `actions/setup-dotnet@v4` to .NET 11, derives the base URL from `${{ github.event.repository.name }}` so the same file works on forks and renames, runs `dotnet run -- build "$BASE_URL"`, writes `.nojekyll`, and hands `output/` to `actions/upload-pages-artifact@v3` and `actions/deploy-pages@v4`._

```yaml:path
examples/SubPathDeployableExample/.github/workflows/deploy.yml
```

### 3. Point the `--project` path at your site

_One to two sentences. The template targets `examples/SubPathDeployableExample`; edit the `--project` argument and any `working-directory` references so the `dotnet run` step points at your csproj. If your repo hosts multiple buildable projects, also consider pinning `--configuration Release` (already set) and adding `actions/cache@v4` over `~/.nuget/packages` — skip both if your restore already takes under a minute._

### 4. Keep `.nojekyll` in the artifact

_One to two sentences. GitHub Pages runs content through Jekyll by default, which silently strips any path starting with an underscore — that would eat Pennington's `_content/` copy folder and SPA `_spa-data/` payloads. The `touch output/.nojekyll` step in the workflow disables Jekyll processing; leave it in unless you have a documented reason to remove it._

### 5. Match the build `baseUrl` to the Pages URL

_Two sentences. Project Pages sites serve at `https://<user>.github.io/<repo>/`, so the workflow passes `/<repo>` as the first positional `build` argument and `BaseUrlHtmlRewriter` prefixes every internal `href`, `src`, and `action` on the way out. If your site sits at an org-level `<user>.github.io` root or a custom apex domain, replace the `BASE_URL` env with an empty string and drop the argument entirely — sub-path wiring has its own how-to at [_Host under a sub-path (base URL)_](xref:how-to.deployment.base-url)._

```csharp:xmldocid
T:Pennington.Infrastructure.BaseUrlHtmlRewriter
```

### 6. (Optional) Fail CI on a bad `BuildReport`

_One to two sentences. The default `RunOrBuildAsync` already sets a non-zero exit code on errors, so the workflow fails fast on broken pages. If you want stricter semantics — for example, failing the main-branch build on broken xrefs while letting warnings pass on feature branches — wrap the call yourself and write the report to stdout._

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)
```

---

## Verify

- Push to `main`; the **Deploy to GitHub Pages** workflow runs the `build` and `deploy` jobs in sequence and turns green.
- Visit `https://<user>.github.io/<repo>/` — the landing page loads, navigation links resolve under `/<repo>/`, and view-source shows `<body data-base-url="/<repo>/">`.
- Open the **build** job log — expect the `BuildReport` summary line with zero failed pages and zero broken links; any non-zero count fails the job.

## Related

- Recipe: [_Build a static site_](xref:how-to.deployment.static-build) — what `build [baseUrl] [outputDirectory]` produces before you automate it.
- Recipe: [_Host under a sub-path (base URL)_](xref:how-to.deployment.base-url) — how `BaseUrlHtmlRewriter` handles the `/<repo>/` prefix for non-GitHub-Pages hosts.
- Recipe: [_Adapt the deploy workflow for other hosts_](xref:how-to.deployment.adapt-for-other-hosts) — Azure Static Web Apps, Cloudflare Pages, and Netlify deltas against this workflow.
- Reference: [_CLI and build arguments_](xref:reference.host.cli) — the `build [baseUrl] [outputDirectory]` surface this workflow drives.
- Reference: [_Build report fields_](xref:reference.diagnostics.build-report) — `BuildReport`, `BuildDiagnostic`, and `BrokenLink` semantics for the CI step above.
