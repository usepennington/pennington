---
title: "Thicket enters public beta"
description: "Rowan Vance announces the public beta of Thicket, Bramble's package manager, covering the registry at thicket.dev, the bramble.toml manifest, publishing, and the lockfile."
date: 2023-06-15
author: Rowan Vance
tags: [thicket, packages, release, tooling]
uid: bramble.blog.thicket-public-beta
---

Starting today, Thicket — Bramble's package manager — is in public beta. You can browse packages at [thicket.dev](https://thicket.dev), publish your own, and pull dependencies into any project using a `bramble.toml` manifest. After months of internal testing with a handful of early adopters, we're ready for the wider community to kick the tires.

## The manifest

Every Bramble project that uses packages has a `bramble.toml` at its root. It describes who you are, what you depend on, and any metadata the registry needs:

```toml
[package]
name    = "fernweb-router"
version = "0.3.0"
authors = ["Indira Cole <indira@fernweb.io>"]
license = "MIT"

[dependencies]
http    = "^1.0"
logging = "~0.9"
```

Version constraints follow a caret/tilde scheme that should feel familiar. `^1.0` means any compatible 1.x, `~0.9` means patch-level updates only. The full reference is in the [bramble.toml reference](xref:bramble.reference.config.bramble-toml).

## Lockfiles

Every `thicket install` produces (or updates) a `bramble.lock` file. The lockfile records the exact resolved version of every package in the transitive graph, including content hashes. Commit it. Future installs — on your machine, in CI, on a colleague's laptop — will resolve identically.

```text
[package.http]
version = "1.2.3"
hash    = "sha256:a3f8c..."

[package.logging]
version = "0.9.11"
hash    = "sha256:7b2e1..."
```

If two packages in your graph depend on the same package at incompatible versions, Thicket tells you at install time rather than letting you discover it at runtime.

## Publishing

Publishing to the registry is a single command once you've authenticated:

```bash
thicket login
thicket publish
```

Thicket reads your `bramble.toml`, packages your source, and uploads it. The registry runs a sandbox check on every upload — your package's public API is extracted and stored, which powers the documentation viewer on thicket.dev automatically. You don't maintain a separate docs site.

> [!NOTE]
> During the beta, all packages are public. Private package support is on the roadmap for before 1.0.

## What the beta means

Beta means the core workflow — init, add, install, publish — is stable enough for real projects. It does not mean the API is frozen. The `thicket.toml` format might grow new fields. Some CLI flags may change names. We'll call out breaking changes clearly in the changelog.

If you hit a bug, use `thicket bug-report` to capture the relevant diagnostic output. If you find a package name you want squatted before the beta ends, now is the time.

The [packaging tutorial](xref:bramble.tutorials.packaging-with-thicket) walks through creating a package from scratch through to publishing. The [Thicket CLI reference](xref:bramble.reference.cli.thicket) covers every subcommand in detail.

We're genuinely excited to see what people build. The whole point of a package ecosystem is that it grows faster than any single team can manage.
