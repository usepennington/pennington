---
title: "Deploy to Cloudflare Pages"
description: "Configure Cloudflare Pages to run the Pennington static build and serve the emitted output directory."
section: "deployment"
order: 50
tags: []
uid: how-to.deployment.cloudflare-pages
isDraft: true
search: false
llms: false
---

> **In this page.** The Cloudflare Pages build settings the static build reads. (Pennington itself reads no environment variables at build time — narrowed from TOC: the static build is configured entirely via CLI positional arguments, so this page documents the build command + output directory fields and notes that any Cloudflare Pages env vars surface as OS env vars to the `dotnet` process but are not consumed by Pennington core.)
>
> **Not in this page.** Workers-based dynamic augmentation (Cloudflare Workers sitting in front of the static output to rewrite, authenticate, or generate responses at the edge).

## When to use this

- Outline bullet: You have a working Pennington site locally (dev serve runs, `dotnet run -- build` produces `output/`) and want it hosted on Cloudflare Pages.
- Outline bullet: You want a recipe for the Pages dashboard fields — not a tour of Cloudflare's product.
- Outline bullet: If you have not yet produced a static build, do the Getting Started tutorial first and come back.

## Assumptions

- Outline bullet: An existing Pennington site whose `Program.cs` ends with `await app.RunOrBuildAsync(args);`.
- Outline bullet: The site builds locally with `dotnet run --project <path> -- build` and emits an `output/` directory containing `index.html` and assets.
- Outline bullet: A Cloudflare account with Pages enabled and the repository pushed to GitHub/GitLab (or connected via Direct Upload).
- Outline bullet: `global.json` or a project-level `TargetFramework` that Cloudflare's build image can satisfy (see Configure the build environment step).
- Outline bullet: Reference example for wiring shape: `examples/MinimalExample/` (a DocSite) or `examples/AlexBlogExample/` (a BlogSite) — do not walk through them, only mirror the `RunOrBuildAsync(args)` call shape.

---

## Steps

### 1. Confirm the build command locally

- Outline bullet: From the repo root, run the build with an explicit base URL and output directory so the output matches what Cloudflare will serve.
- Outline bullet: Command shape (plain fence, not xmldocid — this is a CLI invocation, not a symbol):

```bash
dotnet run --project src/MySite --configuration Release -- build / output
```

- Outline bullet: Expect the build report to print and exit 0; expect `src/MySite/output/index.html` to exist.
- Outline bullet: The three positional tokens after `--` are parsed by `OutputOptions.FromArgs`: `args[0]` must be `build`, `args[1]` is `BaseUrl` (default `/`), `args[2]` is `OutputDirectory` (default `output`). Anything else is ignored.

### 2. Pick the Cloudflare Pages framework preset

- Outline bullet: In the Pages dashboard, choose "Create a project" → "Connect to Git" → select the repository.
- Outline bullet: Framework preset: **None**. Pennington is not in Cloudflare's preset list; leaving it on a preset like Hugo/Jekyll injects wrong defaults.
- Outline bullet: Production branch: `main` (or whichever branch you publish from).

### 3. Fill the Build settings

- Outline bullet: Build command — invokes the same `build` verb you ran locally. Use plain fences (these are Pages form field values, not C# symbols):

```text
Build command:
  dotnet run --project src/MySite --configuration Release -- build / output

Build output directory:
  src/MySite/output

Root directory (advanced):
  /
```

- Outline bullet: The first positional after `--` (`build`) is load-bearing: without it, `RunOrBuildAsync` starts the dev host and never exits.
- Outline bullet: `BaseUrl` is `/` when your Pages project is served at the apex (e.g., `mysite.pages.dev` or a custom domain at root). Set it to `/subpath/` only if the site is mounted under a sub-path.
- Outline bullet: `Build output directory` must point to the same folder the CLI third argument produced — if you pass `dist` as `args[2]`, set this field to `src/MySite/dist`.

### 4. Configure the build environment

- Outline bullet: Cloudflare Pages' default build image does not ship .NET 11. Pin a .NET SDK via a Pages environment variable so the image installs it:

```text
Environment variable name   Value
UNSTABLE_PRE_BUILD          curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 11.0 --install-dir $HOME/.dotnet && export PATH="$HOME/.dotnet:$PATH"
```

- Outline bullet: Alternatively, commit a `global.json` at the repo root pinning the SDK major band, then add a `Pages → Settings → Build & deployments → Build image` selection (V2 build image) so Cloudflare honours it.
- Outline bullet: Pennington core itself reads **no** environment variables at build time (verified against `src/Pennington/`) — the only env var it touches anywhere is `DOTNET_WATCH`, which is dev-serve only. Anything you set in Pages > Environment variables is available to `dotnet` as OS env vars and can be consumed by your own `Program.cs` if you choose, but none are required to produce a correct build.

### 5. Trigger and verify the first deploy

- Outline bullet: Push a commit to the production branch (or click "Save and Deploy") to start the build.
- Outline bullet: Watch the build log: expect the `dotnet run ... build` line, then the Pennington build report, then `Deploying to Cloudflare's global network`.
- Outline bullet: On success Cloudflare prints a preview URL like `https://<project>.pages.dev`.

---

## Verify

- Outline bullet: Open `https://<project>.pages.dev/` — expect the site's home page rendered, not Cloudflare's default welcome.
- Outline bullet: Open `https://<project>.pages.dev/sitemap.xml` — expect a sitemap generated at build time.
- Outline bullet: View the page source — internal `<a>` hrefs should be root-relative (`/foo/`) when `BaseUrl` is `/`, or prefixed (`/subpath/foo/`) when `BaseUrl` is a sub-path.

## Related

- Reference: [CLI build arguments](/reference/cli/build) — the `build [baseUrl] [outputDirectory]` surface.
- Reference: [`OutputOptions`](/reference/options/output-options) — properties consumed by the build.
- Background: [Dev-serve vs. build — one code path](/explanation/architecture/unified-build) — why the CLI positional args are the only knobs.
