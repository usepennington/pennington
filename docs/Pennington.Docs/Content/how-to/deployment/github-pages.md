---
title: "Deploy to GitHub Pages"
description: "Ship a Pennington site to GitHub Pages with a ready-to-copy Actions workflow, base-URL injection, and the `.nojekyll` marker."
uid: how-to.deployment.github-pages
order: 2
sectionLabel: "Publishing & Deployment"
tags: [deployment, github-pages, ci, base-url]
---

This guide covers deploying a working Pennington site committed to a GitHub repo, so Pages builds and deploys it automatically on every push to `main`. When the site still only runs under `dotnet run`, complete <xref:how-to.deployment.static-build> first — the directory structure of `output/` is easier to automate once it's familiar.

## Before you begin
- A Pennington site that builds locally with `dotnet run --project <your-project> -- build` (see [Build a static site](xref:how-to.deployment.static-build) if not).
- The repo is pushed to GitHub and Pages is enabled under **Settings → Pages → Build and deployment → Source: GitHub Actions**.
- The site will serve under a repository sub-path like `https://<user>.github.io/<repo>/`. Root-domain deployments are called out in Step 5.

For a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample) — the `.github/workflows/deploy.yml` and `BuildHost` helper are the relevant siblings.

---

## Steps

<Steps>
<Step StepNumber="1">

**Enable GitHub Pages with the Actions source**

In the repo settings, switch **Pages → Build and deployment → Source** to **GitHub Actions** so the deploy workflow is authorized to publish. Also confirm the three workflow permissions the deploy action needs — `contents: read`, `pages: write`, `id-token: write` — are not blocked at the organization level. The workflow declares them explicitly, but an org-wide deny overrides that.

</Step>
<Step StepNumber="2">

**Add the deploy workflow**

Commit the YAML below to `.github/workflows/deploy.yml` at the repo root. It pins `actions/setup-dotnet@v4` to .NET 10, derives the base URL from `${{ github.event.repository.name }}` so the same file works on forks and renames, runs `dotnet run -- build "$BASE_URL"`, writes `.nojekyll`, and hands `output/` to `actions/upload-pages-artifact@v3` and `actions/deploy-pages@v4`.

```yaml:symbol
examples/SubPathDeployableExample/.github/workflows/deploy.yml
```

> [!NOTE]
> The `touch output/.nojekyll` step is load-bearing: without it GitHub Pages runs the artifact through Jekyll, which strips any path starting with an underscore — including Pennington's `_content/` static-web-asset folder. The marker disables Jekyll so `_content/*` ships verbatim.

</Step>
<Step StepNumber="3">

**Point the `--project` path at your site**

The template targets `examples/SubPathDeployableExample`; edit the `--project` argument and any `working-directory` references so the `dotnet run` step points at the correct csproj.

</Step>
<Step StepNumber="4">

**Match the build `baseUrl` to the Pages URL**

Project Pages sites serve at `https://<user>.github.io/<repo>/`, so the workflow passes `/<repo>` as the first positional `build` argument and `BaseUrlHtmlRewriter` prefixes every internal `href`, `src`, and `action` on the way out. For sites at an org-level root (`https://<org>.github.io/`) or a custom apex domain, the site serves from `/`: set `BASE_URL` to an empty string so `build "$BASE_URL"` passes an empty argument and the rewriter leaves links untouched. The workflow's header comment marks the same two lines to change. Sub-path wiring is covered in <xref:how-to.deployment.base-url>.

</Step>
</Steps>

## Customize the exit semantics

`RunOrBuildAsync` already sets a non-zero exit code on errors, so the workflow above fails fast on broken pages. When you need stricter or more selective behavior — failing the main-branch build on broken xrefs while letting warnings pass on feature branches — skip the `RunOrBuildAsync` extension, run the generator yourself, and inspect the `BuildReport` before setting the exit code. The `BuildHost` helper in the example does exactly that:

```csharp:symbol
examples/SubPathDeployableExample/BuildHost.cs > BuildHost.PrintBuildReport
```

`report.HasErrors` covers broken xrefs and failed pages; branch on `report.Diagnostics` for finer-grained rules. Call `BuildHost.RunOrBuildAsync` from `Program.cs` in place of the default extension to route the build through it.

---

## Verify

- Push to `main`; the **Deploy to GitHub Pages** workflow runs the `build` and `deploy` jobs in sequence and turns green.
- Visit `https://<user>.github.io/<repo>/` — the landing page loads, navigation links resolve under `/<repo>/`, and view-source shows `<body data-base-url="/<repo>">` (the rewriter trims the trailing slash).
- Open the **build** job log — expect the `BuildReport` summary line with zero failed pages and zero broken links; any non-zero count fails the job.

## Related

- Recipe: [Build a static site](xref:how-to.deployment.static-build) — what `build [baseUrl] [outputDirectory]` produces before you automate it.
- Recipe: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url) — how `BaseUrlHtmlRewriter` handles the `/<repo>/` prefix for non-GitHub-Pages hosts.
- Recipe: [Adapt the deploy workflow for other hosts](xref:how-to.deployment.adapt-for-other-hosts) — Azure Static Web Apps, Cloudflare Pages, and Netlify deltas against this workflow.
- Reference: [CLI and build arguments](xref:reference.host.cli) — the `build [baseUrl] [outputDirectory]` surface this workflow drives.
- Reference: [Build report fields](xref:reference.api.build-report) — the `BuildReport` surface (`HasErrors`, `FailedPages`, `Diagnostics`) the CI step above checks.
