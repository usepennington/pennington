---
title: "Run Bramble in CI"
description: "Set up a CI pipeline that installs Bramble, enforces formatting, runs tests, and executes a Trellis build task."
uid: bramble.how-to.tooling.run-in-ci
order: 530
sectionLabel: "Tooling"
tags: [ci, github-actions, gitlab-ci, testing, trellis]
---

A standard Bramble CI pipeline has three stages: format check, test, and build. Running `sprig fmt --check` first catches style drift cheaply; `bramble test` validates correctness; `trellis run build` produces release artifacts.

## Install Bramble in CI

The official install script reads the version from `bramble.toml` when `BRAMBLE_VERSION` is not set explicitly, so pinning the version in your manifest is enough:

```toml
# bramble.toml
[workspace]
bramble = "1.2.4"
```

Both pipeline examples below use this approach.

## Pipeline definitions

```yaml tabs=true title="GitHub Actions"
name: CI

on:
  push:
    branches: [main]
  pull_request:

jobs:
  ci:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Install Bramble
        run: curl -fsSL https://install.bramble.dev | sh
        env:
          BRAMBLE_VERSION: ${{ '{{' }} fromTOML(readFile('bramble.toml')).workspace.bramble {{ '}}' }}

      - name: Check formatting
        run: sprig fmt --check

      - name: Run tests
        run: bramble test --reporter tap

      - name: Build
        run: trellis run build
```

```yaml tabs=true title="GitLab CI"
default:
  image: debian:bookworm-slim

stages:
  - lint
  - test
  - build

.install_bramble: &install_bramble
  before_script:
    - apt-get update -qq && apt-get install -y -qq curl
    - curl -fsSL https://install.bramble.dev | sh
    - export PATH="$HOME/.bramble/bin:$PATH"

fmt:
  stage: lint
  <<: *install_bramble
  script:
    - sprig fmt --check

test:
  stage: test
  <<: *install_bramble
  script:
    - bramble test --reporter tap

build:
  stage: build
  <<: *install_bramble
  script:
    - trellis run build
  artifacts:
    paths:
      - dist/
```

## Cache the toolchain

Both platforms support dependency caching. Cache `~/.bramble/cache` to avoid re-downloading packages on every run:

```bash
# key on the lockfile so the cache invalidates when dependencies change
key: bramble-${{ hashFiles('bramble.lock') }}
path: ~/.bramble/cache
```

> [!NOTE]
> The install script is idempotent. If the requested version is already present in the cache, it skips the download and exits immediately.

## Fail fast on lint errors

If you want format failures to block the test stage, keep `sprig fmt --check` and `bramble test` in separate jobs with a dependency chain rather than combining them in one step. This gives clearer per-stage status in the UI and lets reviewers see exactly which check failed.

For the full set of `bramble test` flags, see the [Bramble CLI reference](xref:bramble.reference.cli.bramble). For defining the `build` task that `trellis run build` executes, see the [Bramble tutorials](xref:bramble.tutorials.index).
