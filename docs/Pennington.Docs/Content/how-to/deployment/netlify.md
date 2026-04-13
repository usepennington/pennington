---
title: Deploy to Netlify
description: Configure a Netlify build to publish a Pennington site via the build command, publish directory, and a minimal netlify.toml.
section: deployment
order: 40
tags: []
uid: how-to.deployment.netlify
isDraft: true
search: false
llms: false
---

> **In this page.** Configure a Netlify build to publish a Pennington site via the build command, publish directory, and a minimal `netlify.toml`.
>
> **Not in this page.** Netlify Functions and edge middleware are out of scope; Pennington builds a static site, so neither is involved.

## When to use this

- Outline bullet: You have a working Pennington site locally and want Netlify to produce the production artifact on every push.
- Outline bullet: Name the concrete endpoint — a Netlify-hosted static site served from `output/`.
- Outline bullet: Point readers arriving too early at the Getting Started tutorial.

## Assumptions

- Outline bullet: Existing Pennington site runs locally via `dotnet run`.
- Outline bullet: A Netlify account and a connected Git repository.
- Outline bullet: A `global.json` (or project-level SDK setting) pinning the .NET 11 SDK Netlify should install.
- Outline bullet: You know your canonical base URL (for non-root hosting) — defaults to `/`.
- Outline bullet: Reference working samples under `examples/` for shape, not a walkthrough.

---

## Steps

### 1. Confirm the Pennington build command

- Outline bullet: Verify `dotnet run --project <site> -- build [baseUrl] [outputDirectory]` is how Pennington emits static output.
- Outline bullet: Note: `args[0]` must be `build`; `args[1]` defaults to `/`; `args[2]` defaults to `output`.
- Outline bullet: Source of truth: `src/Pennington/Generation/OutputOptions.cs` (`FromArgs`).
- Outline bullet: The build reuses the same HTTP pipeline as dev serve — no separate renderer path.

### 2. Pin the .NET SDK for Netlify

- Outline bullet: Add a `global.json` at the repo root pinning the major SDK so Netlify's image resolves a compatible version.
- Outline bullet: Ensure `DOTNET_NOLOGO=1` and `DOTNET_CLI_TELEMETRY_OPTOUT=1` to keep logs clean.
- Outline bullet: Snippet: plain `json` fence showing a `global.json` with `sdk.version` and `rollForward`.

### 3. Write `netlify.toml` at the repo root

- Outline bullet: Use a plain `toml` fence.
- Outline bullet: Set `[build] command` to invoke `dotnet run --project <site-project> -- build` with the desired `baseUrl` and `output` positional args.
- Outline bullet: Set `[build] publish` to the directory passed as `args[2]` (e.g. `src/MySite/output`).
- Outline bullet: Under `[build.environment]` pin `DOTNET_VERSION` to match `global.json`.
- Outline bullet: Add a `[[redirects]]` rule for SPA-style fallback to `/404.html` with `status = 404` (Pennington emits `404.html` via the `/__pennington-404-generator` sentinel).

### 4. Match the publish directory to `OutputOptions`

- Outline bullet: `publish` in `netlify.toml` must equal the `args[2]` path passed on the build command (resolved relative to the project directory).
- Outline bullet: Default when omitted is `output` — prefer setting it explicitly so the TOML and CLI stay in sync.
- Outline bullet: If hosting under a sub-path, pass that as `args[1]` so internal links are rewritten by `BaseUrlHtmlRewriter`.

### 5. Commit and connect the repo in Netlify

- Outline bullet: Push `netlify.toml` and `global.json`.
- Outline bullet: In Netlify, "Add new site -> Import from Git" and leave build settings blank — `netlify.toml` wins.
- Outline bullet: Trigger the first deploy and watch the build log confirm the `dotnet run ... -- build` invocation.

---

## Verify

- Outline bullet: Netlify deploy log shows the `build` command succeeding with zero errors in the `BuildReport`.
- Outline bullet: The published site loads at the Netlify URL; internal links include the configured `baseUrl`.
- Outline bullet: Hitting an unknown path returns the generated `404.html`.

## Related

- Reference: `OutputOptions` and the `build` CLI surface (link to the deployment reference page once published).
- Background: "How dev-serve and build share one pipeline" explanation.
