---
title: "Use a private registry"
description: "How to configure Thicket to resolve packages from a private registry using a URL and authentication token."
uid: bramble.how-to.thicket.use-a-private-registry
order: 230
sectionLabel: "Thicket"
tags: [thicket, registry, authentication, private, configuration]
---

Private registries let teams host internal packages without exposing them on `thicket.dev`. Thicket supports multiple active registries simultaneously and resolves each package from the first registry that declares it.

## Declare the registry in bramble.toml

Add a `[[registry]]` block for each private source. The `url` field is required; `name` is the alias you use in scope prefixes.

```toml
[[registry]]
name = "acme"
url  = "https://packages.acme.internal/thicket"

[[registry]]
name = "thicket"
url  = "https://registry.thicket.dev"
```

Registries are tried in declaration order. List more-specific (private) registries first so internal packages are not shadowed by public ones.

> [!TIP]
> You can omit the public registry entry entirely if all your dependencies are internal. Thicket only contacts registries you declare.

## Scope packages to a registry

Prefix a package name with `<registry-name>:` to pin it to a specific registry regardless of order:

```toml
[dependencies]
"acme:ui-kit"    = "^3.1.0"
bramble-slug     = "^1.2.0"    # resolved from whichever registry lists it first
```

## Authenticate with an API token

Tokens can be stored in the per-user Thicket config file at `~/.config/thicket/credentials.toml`:

```toml
[registries.acme]
token = "thk_live_s3cr3t_abc123"
```

Do not commit this file. For CI and shared environments, use environment variables instead.

## Set credentials via environment variables

Thicket reads `THICKET_REGISTRY_<NAME>_TOKEN` for each named registry, where `<NAME>` is the registry name uppercased with hyphens replaced by underscores:

```bash
export THICKET_REGISTRY_ACME_TOKEN="thk_live_s3cr3t_abc123"
thicket install
```

This approach keeps secrets out of files on disk and works naturally with secret-injection systems in CI pipelines. See [running Bramble in CI](xref:bramble.how-to.tooling.run-in-ci) for a complete CI example.

## Verify the configuration

After adding the registry, confirm resolution works:

```bash
thicket resolve acme:ui-kit
```

Thicket prints the resolved version and the registry it came from:

```text
acme:ui-kit 3.2.1 (from https://packages.acme.internal/thicket)
```

If authentication fails, Thicket prints error `B0203` (registry authentication failure) along with the URL it attempted. Double-check the token value and that it has read access to the registry.

## Mirror a private registry for offline builds

Once dependencies resolve correctly, consider [vendoring them](xref:bramble.how-to.thicket.vendor-dependencies-offline) so builds remain reproducible even when the private server is unreachable.
