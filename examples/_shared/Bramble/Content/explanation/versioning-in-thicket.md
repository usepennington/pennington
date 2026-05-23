---
title: "Versioning in Thicket"
description: "Thicket uses semantic versioning with a SAT-based resolver and a committed lockfile to give reproducible, conflict-minimising dependency graphs."
uid: bramble.explanation.versioning-in-thicket
order: 110
sectionLabel: "Explanation"
tags: [thicket, versioning, semver, lockfile, dependencies]
---

Thicket's versioning model is built on two ideas: version constraints declare intent, and the lockfile records what actually happened. The resolver bridges them, finding a concrete assignment of versions that satisfies every constraint in the graph. Understanding how each layer works helps explain why the model behaves the way it does at the edges.

## Semantic versioning as a contract

Every package on `thicket.dev` carries a semantic version. Thicket treats the major version as a compatibility boundary: a `2.x` package and a `1.x` package of the same name are considered different packages by the resolver. Within a major version, minor releases may add API surface and patch releases may only fix bugs — this is a social contract enforced by tooling that can flag breaking changes, not a hard constraint the language itself imposes.

Constraints in `bramble.toml` follow standard semver range syntax:

```toml
[dependencies]
http_client = "^2.1"
json        = "~1.8.3"
```

`^2.1` allows any `2.x` at or above `2.1`; `~1.8.3` allows only patch-level updates within `1.8`.

## The resolver

When you run `thicket add` or `thicket install`, Thicket builds a constraint graph from your direct dependencies and their transitive requirements, then runs a resolver to find a single version for each package that satisfies all constraints simultaneously.

Thicket's resolver prefers the newest version that satisfies all constraints, breaking ties toward fewer total packages. When no single version can satisfy all constraints — a genuine conflict — the resolver stops and reports which packages are in tension, making the conflict visible rather than picking a winner silently.

## When duplicate versions are permitted

There is one deliberate exception: if two packages depend on incompatible major versions of the same library, Thicket allows both to coexist. Each dependent sees its own major version; the packages are physically distinct entries in the lockfile. This is pragmatic — it avoids making a diamond-dependency conflict a hard blocker — but it comes with a cost: the two versions cannot exchange types across the boundary, and binary size grows.

Duplicate versions appear as an informational warning during installation. Accepting them is a conscious choice, not a default you drift into.

## The lockfile's role

Once the resolver has produced a concrete assignment, Thicket writes `thicket.lock`. This file records exact versions, content hashes, and source roots for every package in the graph. Subsequent installs on any machine reproduce the exact same graph without re-running the resolver, as long as the lockfile is present and committed.

The lockfile should be committed to version control for applications. For libraries, committing the lockfile is optional — consumers will resolve their own graph — though it remains useful for CI.

For day-to-day tasks around managing versions see [Pin and update versions](xref:bramble.how-to.thicket.pin-and-update-versions). The full `bramble.toml` schema, including all constraint operators, is in the [bramble.toml reference](xref:bramble.reference.config.bramble-toml).
