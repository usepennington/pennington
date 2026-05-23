---
title: "Publish a package"
description: "How to prepare package metadata, authenticate with Thicket, and publish a release to the registry."
uid: bramble.how-to.thicket.publish-a-package
order: 210
sectionLabel: "Thicket"
tags: [thicket, publishing, packages, registry, versioning]
---

Before anyone can `thicket add` your library, you need a well-formed `bramble.toml`, a Thicket account, and one command. This guide walks through each step from a local project to a published package.

## Fill in package metadata

The `[package]` table in `bramble.toml` is the canonical identity of your package. Every field below is required for publishing.

```toml
[package]
name        = "bramble-slug"          # globally unique on thicket.dev
version     = "1.0.0"                 # semver
description = "URL-safe slug generator for Bramble"
license     = "MIT"
authors     = ["Ada Thornwood <ada@thornwood.dev>"]
repository  = "https://vcs.example.com/ada/bramble-slug"

[package.exports]
main = "src/lib.br"
```

The `name` must be lowercase, may contain hyphens, and must not already exist in the registry under a different owner. Check availability with `thicket search <name>` before you commit to it.

> [!NOTE]
> If your package targets a minimum Bramble version, add `bramble = ">=1.2"` under `[package]`. Thicket enforces this constraint at install time so consumers get a clear error instead of a cryptic runtime failure.

## Authenticate with Thicket

Generate an API token at `https://thicket.dev/settings/tokens`, then log in on the machine you publish from:

```bash
thicket login --token $THICKET_TOKEN
```

Credentials are stored in `~/.config/thicket/credentials.toml`. On CI systems, pass the token via the `THICKET_TOKEN` environment variable instead — see [running Bramble in CI](xref:bramble.how-to.tooling.run-in-ci) for the recommended approach.

## Publish the package

With metadata complete and credentials saved, publish from the project root:

```bash
thicket publish
```

Thicket performs a pre-flight check — it validates the manifest, ensures `version` is not already taken in the registry, and confirms that the declared `main` file exists. If any check fails, it prints a diagnostic and exits before uploading anything.

On success the output looks like:

```text
Packing bramble-slug@1.0.0...
Verifying manifest...
Uploading (4.2 KB)...
Published bramble-slug@1.0.0 to thicket.dev
```

## Bump the version for subsequent releases

Thicket will reject a publish if the version already exists in the registry. Edit `bramble.toml` manually or use the bump subcommand:

```bash
thicket version patch    # 1.0.0 → 1.0.1
thicket version minor    # 1.0.0 → 1.1.0
thicket version major    # 1.0.0 → 2.0.0
```

This edits `bramble.toml` in place. Commit the change before publishing so your VCS history stays aligned with the registry.

## Publish a pre-release

Append a pre-release identifier to signal instability:

```bash
thicket version prerelease --tag beta   # 1.1.0 → 1.1.0-beta.1
thicket publish
```

Pre-release versions are not installed by default; consumers must opt in with an explicit version constraint. Refer to [pinning and updating versions](xref:bramble.how-to.thicket.pin-and-update-versions) for how dependents express that intent.
