---
title: "Vendor dependencies for offline builds"
description: "How to copy resolved dependencies into a vendor directory so builds work without network access."
uid: bramble.how-to.thicket.vendor-dependencies-offline
order: 240
sectionLabel: "Thicket"
tags: [thicket, vendor, offline, reproducibility, dependencies]
---

Vendoring copies every resolved dependency into a `vendor/` directory in your project. Once vendored, `thicket install --offline` uses only those local copies, making builds fully reproducible with no registry calls.

## Run thicket vendor

From the project root, with a valid `thicket.lock` in place:

```bash
thicket vendor
```

Thicket reads the lockfile, downloads each package at its pinned version, and writes the source trees under `vendor/`:

```text
vendor/
  bramble-slug@1.2.3/
    bramble.toml
    src/
  http-client@1.6.0/
    bramble.toml
    src/
  thicket.vendor.json     ← manifest of vendored contents
```

`thicket.vendor.json` records the content hashes Thicket uses to detect tampering or accidental edits.

> [!NOTE]
> If you're using a private registry, run `thicket vendor` on a machine that has network access and valid credentials. The resulting `vendor/` directory contains the resolved source and can be moved anywhere.

## Install from the vendor directory

Pass `--offline` to any install or build command to prevent Thicket from making network requests:

```bash
thicket install --offline
```

If a package in `thicket.lock` is absent from `vendor/`, the command fails immediately with a clear diagnostic listing the missing entries rather than attempting a network fetch.

## Commit vendor/ to version control

Whether to commit `vendor/` is a team decision. Common patterns:

| Scenario | Recommendation |
|---|---|
| Air-gapped CI environment | Commit `vendor/` |
| Reproducibility without commit churn | Commit `thicket.lock`, generate `vendor/` in CI from it |
| Auditing supply chain | Commit `vendor/` and review diffs on updates |

If you commit `vendor/`, add a note in your `README` so contributors know to run `thicket vendor` after updating `thicket.lock`.

## Update vendored dependencies

After changing version constraints and running `thicket update`, refresh the vendor directory:

```bash
thicket update
thicket vendor
```

The `vendor/` directory is fully replaced. Review the diff before committing — the changed files directly reflect which packages changed and what source code was added or removed.

See [pinning and updating versions](xref:bramble.how-to.thicket.pin-and-update-versions) for how to control which versions get resolved before vendoring.

## Verify vendor integrity

At any point, check that `vendor/` matches `thicket.vendor.json`:

```bash
thicket vendor --verify
```

This is useful in CI to catch accidental edits to vendored sources without running a full build.
