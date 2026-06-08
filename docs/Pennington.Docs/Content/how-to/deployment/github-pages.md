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

Commit the YAML below to `.github/workflows/deploy.yml` at the repo root. It pins `actions/setup-dotnet@v4` to .NET 11, derives the base URL from `${{ github.event.repository.name }}` so the same file works on forks and renames, runs `dotnet run -- build "$BASE_URL"`, writes `.nojekyll`, and hands `output/` to `actions/upload-pages-artifact@v3` and `actions/deploy-pages@v4`.

```yaml:symbol
examples/SubPathDeployableExample/.github/workflows/deploy.yml
```

</Step>
<Step StepNumber="3">

**Point the `--project` path at your site**

The template targets `examples/SubPathDeployableExample`; edit the `--project` argument and any `working-directory` references so the `dotnet run` step points at the correct csproj.

</Step>
<Step StepNumber="4">

**Keep `.nojekyll` in the artifact**

GitHub Pages would otherwise run content through Jekyll, which strips paths starting with an underscore — including Pennington's `_content/` static-web-asset folder. The `touch output/.nojekyll` step in the workflow disables Jekyll processing.

</Step>
<Step StepNumber="5">

**Match the build `baseUrl` to the Pages URL**

Project Pages sites serve at `https://<user>.github.io/<repo>/`, so the workflow passes `/<repo>` as the first positional `build` argument and `BaseUrlHtmlRewriter` prefixes every internal `href`, `src`, and `action` on the way out. For sites at an org-level root or a custom apex domain, replace the `BASE_URL` env with an empty string and drop the argument entirely. Sub-path wiring is covered in <xref:how-to.deployment.base-url>.

</Step>
</Steps>

`RunOrBuildAsync` sets a non-zero exit code on errors, so the workflow fails fast on broken pages. For stricter exit semantics (failing the main-branch build on broken xrefs while letting warnings pass on feature branches), wrap the call and write the report to stdout — see `BuildHost.RunOrBuildAsync` and `BuildHost.PrintBuildReport` in `examples/SubPathDeployableExample/BuildHost.cs`.

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
