---
title: "Deploy to GitHub Pages"
description: "Ship a Pennington site to GitHub Pages with a ready-to-copy Actions workflow, base-URL injection, and the `.nojekyll` marker."
uid: how-to.deployment.github-pages
order: 204020
sectionLabel: Publishing & Deployment
tags: [deployment, github-pages, ci, base-url]
---

This guide covers deploying a working Pennington site committed to a GitHub repo, so Pages builds and deploys it automatically on every push to `main`. When the site still only runs under `dotnet run`, complete <xref:how-to.deployment.static-build> first — the shape of `output/` is easier to automate once it's familiar.

## Assumptions

- A Pennington site that builds locally with `dotnet run --project <your-project> -- build` (see [_Build a static site_](xref:how-to.deployment.static-build) if not).
- The repo is pushed to GitHub and Pages is enabled under **Settings → Pages → Build and deployment → Source: GitHub Actions**.
- The site will serve under a repository sub-path like `https://<user>.github.io/<repo>/` — root-domain deployments are called out in Step 5.
- Working with GitHub Actions YAML at the "copy, commit, inspect the run log" level feels approachable.

For a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample). The `.github/workflows/deploy.yml`, host-config siblings (`staticwebapp.config.json`, `netlify.toml`, `nginx.conf`, `web.config`), and the `BuildHost` helper are the teaching surface; the rest of the example is outside scope here.

---

## Steps

<Steps>
<Step StepNumber="1">

**Enable GitHub Pages with the Actions source**

In the repo settings, switch **Pages → Build and deployment → Source** to **GitHub Actions** so the deploy workflow is authorized to publish. Also confirm the three workflow permissions the deploy action needs — `contents: read`, `pages: write`, `id-token: write` — are not blocked at the organization level. The workflow declares them explicitly, but an org-wide deny overrides that.

</Step>
<Step StepNumber="2">

**Drop in the canonical workflow**

Commit the YAML below to `.github/workflows/deploy.yml` at the repo root. It pins `actions/setup-dotnet@v4` to .NET 11, derives the base URL from `${{ github.event.repository.name }}` so the same file works on forks and renames, runs `dotnet run -- build "$BASE_URL"`, writes `.nojekyll`, and hands `output/` to `actions/upload-pages-artifact@v3` and `actions/deploy-pages@v4`.

```yaml:path
examples/SubPathDeployableExample/.github/workflows/deploy.yml
```

</Step>
<Step StepNumber="3">

**Point the `--project` path at your site**

The template targets `examples/SubPathDeployableExample`; edit the `--project` argument and any `working-directory` references so the `dotnet run` step points at the correct csproj. For repos that host multiple buildable projects, add `actions/cache@v4` over `~/.nuget/packages` if NuGet restore takes more than a minute — `--configuration Release` is already set.

</Step>
<Step StepNumber="4">

**Keep `.nojekyll` in the artifact**

GitHub Pages runs content through Jekyll by default, which silently strips any path starting with an underscore — that removes Pennington's `_content/` copy folder and SPA `_spa-data/` payloads. The `touch output/.nojekyll` step in the workflow disables Jekyll processing; leave it in place.

</Step>
<Step StepNumber="5">

**Match the build `baseUrl` to the Pages URL**

Project Pages sites serve at `https://<user>.github.io/<repo>/`, so the workflow passes `/<repo>` as the first positional `build` argument and `BaseUrlHtmlRewriter` prefixes every internal `href`, `src`, and `action` on the way out. For sites at an org-level root or a custom apex domain, replace the `BASE_URL` env with an empty string and drop the argument entirely. Sub-path wiring is covered in <xref:how-to.deployment.base-url>.

</Step>
<Step StepNumber="6">

**(Optional) Fail CI on a bad `BuildReport`**

`RunOrBuildAsync` already sets a non-zero exit code on errors, so the workflow fails fast on broken pages. For stricter semantics — failing the main-branch build on broken xrefs while letting warnings pass on feature branches — wrap the call and write the report to stdout.

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)
```

</Step>
</Steps>

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
