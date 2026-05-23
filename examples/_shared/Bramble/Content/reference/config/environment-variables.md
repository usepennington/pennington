---
title: "Environment variables"
description: "Reference for environment variables that control Bramble toolchain behavior at the process level."
uid: bramble.reference.config.environment-variables
order: 540
sectionLabel: "Configuration"
tags: [configuration, environment, toolchain, cache, registry]
---

The Bramble toolchain reads several environment variables to locate home directories, caches, and registries. Values set in these variables take precedence over compiled-in defaults but are overridden by explicit CLI flags.

## Variables

| Variable | Default | Description |
|---|---|---|
| `BRAMBLE_HOME` | `~/.bramble` | Root directory for toolchain data: installed toolchains, the package cache, and REPL history |
| `BRAMBLE_CACHE` | `$BRAMBLE_HOME/cache` | Directory for compiled artifact caches and thicket package downloads; can be set to a shared path on CI to reuse across builds |
| `THICKET_REGISTRY` | `https://thicket.dev` | Base URL of the package registry. Set this to a private mirror for corporate or air-gapped environments |
| `THICKET_TOKEN` | _(none)_ | Authentication token used by `thicket publish`, `thicket audit`, and private registry fetches. Prefer `thicket login` for interactive use; use this variable in CI pipelines |
| `NO_COLOR` | _(unset)_ | When set to any non-empty value, disables ANSI color output across all Bramble toolchain CLIs, regardless of `--color` flags |

## Usage notes

**BRAMBLE_HOME** controls everything downstream. Changing it moves the package cache, the installed toolchain index, and the REPL history file. On shared CI runners, setting a project-specific `BRAMBLE_HOME` per workspace prevents cache collisions.

**BRAMBLE_CACHE** can be set independently of `BRAMBLE_HOME` to point at a mounted cache volume without relocating other toolchain state.

**THICKET_TOKEN** is read directly from the environment; it is never written to disk by the toolchain. Store it in your CI secret manager and inject it as an environment variable at build time.

**NO_COLOR** follows the [no-color.org](https://no-color.org) convention. Setting it to `"1"`, `"true"`, or any non-empty string is equivalent — only the empty string and an unset variable enable color.

## Example: CI configuration

```bash
export BRAMBLE_HOME=/ci/bramble
export BRAMBLE_CACHE=/mnt/cache/bramble
export THICKET_REGISTRY=https://registry.internal.example.dev
export THICKET_TOKEN=$CI_SECRET_THICKET_TOKEN
export NO_COLOR=1
```

> **Note:** `THICKET_TOKEN` grants write access to any package you own on the registry. Treat it with the same care as an API key — rotate it if it is ever exposed in logs or version control.

See [environment variables in audit workflows](xref:bramble.how-to.thicket.audit-dependencies) for token scoping guidance, and the [bramble CLI reference](xref:bramble.reference.cli.bramble) for flags that override these values at invocation time.
