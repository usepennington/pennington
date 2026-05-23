---
title: "Pin and update versions"
description: "How to control dependency versions with version specifiers, the lockfile, and the thicket update command."
uid: bramble.how-to.thicket.pin-and-update-versions
order: 220
sectionLabel: "Thicket"
tags: [thicket, versions, lockfile, dependencies, semver]
---

Thicket separates _what you're willing to accept_ (version specifiers in `bramble.toml`) from _what you actually have_ (the exact resolved versions in `thicket.lock`). Understanding both lets you control upgrades intentionally rather than accidentally.

## Version specifier syntax

The `[dependencies]` table in `bramble.toml` supports several specifier forms:

```toml
[dependencies]
bramble-slug   = "1.2.3"       # exact pin — only this version
http-client    = "^1.4.0"      # compatible with 1.x.x, >= 1.4.0
color-parser   = "~2.1.0"      # patch-only updates: >= 2.1.0, < 2.2.0
log-formatter  = ">=0.9, <2"   # range: any version satisfying both constraints
dev-helper     = "*"           # any version (rarely advisable in production)
```

`^` is the most common choice for libraries: it allows non-breaking minor and patch updates while locking the major version.

## How thicket.lock works

The first time you run `thicket install` (or `thicket add`), Thicket resolves every constraint, picks exact versions, and writes `thicket.lock`. Subsequent installs read the lockfile directly — no resolution happens, so builds are reproducible.

Commit `thicket.lock` to version control. Removing it forces a full re-resolution on the next install, which may silently bump transitive dependencies.

> [!IMPORTANT]
> Libraries should also commit the lockfile for their own development, but they should **not** enforce it on consumers. The lockfile in a library repo is for reproducible CI, not for constraining downstream users.

## Add a dependency at a specific version

```bash
thicket add bramble-slug@1.2.0
```

This writes `bramble-slug = "1.2.0"` into `[dependencies]` and updates `thicket.lock` immediately. Omit the version to accept the latest compatible release.

```bash
thicket add bramble-slug          # resolves to latest stable
thicket add bramble-slug@^1.4     # adds with a caret constraint
```

## Update dependencies

To update all dependencies within their existing specifiers:

```bash
thicket update
```

To update a single package:

```bash
thicket update bramble-slug
```

This resolves the newest version that still satisfies the specifier in `bramble.toml` and rewrites `thicket.lock`. It does not change the specifier itself.

To widen the specifier and jump to a new major version, edit `bramble.toml` manually and run `thicket install`:

```toml
[dependencies]
http-client = "^2.0.0"    # was ^1.4.0
```

```bash
thicket install
```

## Check what would change before updating

```bash
thicket update --dry-run
```

The output lists each package, the currently locked version, and the version that would be installed:

```text
http-client    1.4.2  →  1.6.0
log-formatter  0.9.1  →  0.9.3  (no change)
```

Pair this with [auditing for vulnerabilities](xref:bramble.how-to.thicket.audit-dependencies) before committing an update to a production lockfile.
