---
title: "thicket"
description: "Reference for the thicket CLI, Bramble's package manager for adding, publishing, and auditing dependencies."
uid: bramble.reference.cli.thicket
order: 420
sectionLabel: "CLI"
tags: [cli, thicket, packages, dependencies, registry]
---

`thicket` is the Bramble package manager. It resolves and fetches packages from the `thicket.dev` registry, manages the `bramble.toml` manifest and `thicket.lock` lockfile, and provides publishing and auditing workflows.

## Subcommands

| Subcommand | Purpose |
|---|---|
| `add` | Add a dependency to `bramble.toml` and update the lockfile |
| `remove` | Remove a dependency from `bramble.toml` and update the lockfile |
| `install` | Fetch all dependencies declared in `bramble.toml` |
| `update` | Upgrade one or all dependencies within their declared constraints |
| `publish` | Package and upload the current package to `thicket.dev` |
| `login` | Authenticate with the registry and store a token |
| `audit` | Check dependencies for known vulnerabilities |
| `vendor` | Copy resolved packages into a local `vendor/` directory |
| `search` | Query the registry for packages by name or keyword |

## Global flags

| Flag | Description |
|---|---|
| `--registry <url>` | Override the default registry (`thicket.dev`) |
| `--token <token>` | Supply an auth token directly (prefer `thicket login`) |
| `--offline` | Use only cached packages; fail if any are missing |
| `--quiet` / `-q` | Suppress progress output |

## thicket add

Adds a package to `[dependencies]` in `bramble.toml` and resolves the lockfile. Use `--dev` to add to `[dev-dependencies]`.

```bash
thicket add thorn/http
thicket add thorn/http@1.4
thicket add --dev bramble/testing-extra
```

## thicket remove

Removes a dependency and any packages that become unreferenced.

```bash
thicket remove thorn/http
```

## thicket install

Downloads and caches all packages listed in `bramble.toml`. Respects `thicket.lock` for exact versions; fails if the lockfile is inconsistent with the manifest.

```bash
thicket install
thicket install --frozen   # fail if lockfile would change
```

## thicket update

```bash
thicket update              # update all packages within constraints
thicket update thorn/http   # update one package
thicket update --breaking   # allow semver-major upgrades
```

## thicket publish

Packages the current directory and uploads it to the registry. Requires prior `thicket login`.

```bash
thicket publish
thicket publish --dry-run   # validate packaging without uploading
```

## thicket login

Stores credentials in the system keyring (or `THICKET_TOKEN` if the keyring is unavailable).

```bash
thicket login
```

```text
Open https://thicket.dev/token in your browser.
Paste your token: ****
Logged in as: ada@example.dev
```

## thicket audit

Checks all resolved packages against the `thicket.dev` advisory database and reports findings by severity.

```bash
thicket audit
thicket audit --deny moderate   # exit non-zero on moderate or above
```

```text
AUDIT REPORT
  thorn/http 1.3.0  [HIGH]   B-ADV-0042: unsafe deserialization in `parse_response`
  bramble/regex 0.9 [LOW]    B-ADV-0017: exponential backtracking on crafted input
2 advisories found.
```

## thicket vendor

Copies all locked packages into `vendor/`. Commit the directory to make builds fully reproducible without network access.

```bash
thicket vendor
thicket vendor --clean   # remove packages no longer in the lockfile
```

## thicket search

```bash
thicket search http
thicket search --limit 10 json
```

```text
thorn/http          1.5.2   HTTP client and server primitives
briar/http-router   0.8.0   Radix-tree request router
```

See [bramble.toml](xref:bramble.reference.config.bramble-toml) for the manifest format and [pinning and updating versions](xref:bramble.how-to.thicket.pin-and-update-versions) for version constraint workflows.
