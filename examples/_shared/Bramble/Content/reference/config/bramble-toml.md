---
title: "bramble.toml"
description: "Reference for the bramble.toml manifest file, which declares package metadata, dependencies, and feature flags."
uid: bramble.reference.config.bramble-toml
order: 510
sectionLabel: "Configuration"
tags: [configuration, manifest, dependencies, features, package]
---

`bramble.toml` is the package manifest. It lives at the root of every Bramble package and is read by `bramble`, `thicket`, and `trellis`. The file uses standard TOML syntax.

## [package]

Declares the package identity and build settings.

| Key | Type | Description |
|---|---|---|
| `name` | string | Package name, scoped as `owner/name` |
| `version` | string | SemVer version string |
| `description` | string | One-sentence summary shown on `thicket.dev` |
| `authors` | array of strings | Author names or `"Name <email>"` entries |
| `license` | string | SPDX license identifier |
| `bramble` | string | Minimum required Bramble version (e.g. `"1.2"`) |
| `entry` | string | Entry-point source file; defaults to `src/main.br` |
| `edition` | string | Language edition: `"2024"` (default) or `"2023"` |

## [dependencies]

Lists runtime dependencies. Keys are package references (`owner/name`); values are version requirements.

| Value form | Example | Meaning |
|---|---|---|
| Exact string | `"1.4.2"` | Exactly version 1.4.2 |
| Caret | `"^1.4"` | Compatible with 1.4, less than 2.0 |
| Tilde | `"~1.4.2"` | Compatible with 1.4.2, less than 1.5 |
| Wildcard | `"1.*"` | Any 1.x release |
| Path dep | `{ path = "../mylib" }` | Local path, not published |

## [dev-dependencies]

Same syntax as `[dependencies]`. Packages listed here are available only during `bramble test` and `trellis` tasks; they are not included in published artifacts.

## [features]

Declares optional compilation features. Each key is a feature name; the value is an array of other features or dependency references it enables.

| Key | Type | Description |
|---|---|---|
| `default` | array | Features enabled when no explicit feature set is given |
| `<name>` | array | Named feature; can list other features or `dep/<pkg>` to enable an optional dep |

## Sample bramble.toml

```toml
[package]
name        = "ada/webserver"
version     = "0.3.1"
description = "A small HTTP server library for Bramble"
authors     = ["Ada Lovelace <ada@example.dev>"]
license     = "MIT"
bramble     = "1.2"
entry       = "src/lib.br"
edition     = "2024"

[dependencies]
"thorn/http"     = "^1.4"
"briar/logging"  = "~0.9.1"

[dev-dependencies]
"bramble/testing-extra" = "^0.5"

[features]
default   = ["tls"]
tls       = ["dep/thorn/http-tls"]
metrics   = []
```

> **Note:** Changing a feature set after `thicket install` requires re-running `thicket install` to update the resolved graph. The lockfile records which features were active at resolution time.

See [thicket add/remove](xref:bramble.reference.cli.thicket) for modifying dependencies from the command line and [pinning and updating versions](xref:bramble.how-to.thicket.pin-and-update-versions) for constraint strategies.
