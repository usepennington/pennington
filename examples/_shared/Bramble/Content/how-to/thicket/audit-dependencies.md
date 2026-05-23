---
title: "Audit dependencies"
description: "How to scan your dependency tree for known advisories using thicket audit and apply available fixes."
uid: bramble.how-to.thicket.audit-dependencies
order: 250
sectionLabel: "Thicket"
tags: [thicket, security, audit, advisories, dependencies]
---

`thicket audit` checks every package in your `thicket.lock` against the Thicket advisory database and reports known vulnerabilities, deprecations, and yanked versions. Run it before releases and in CI to catch issues before they reach production.

## Run an audit

```bash
thicket audit
```

Thicket contacts `https://advisory.thicket.dev`, matches your locked versions against published advisories, and prints a report:

```text
Auditing 14 packages...

CRITICAL  bramble-xml@0.9.1
  Advisory: THKT-2025-0047 — arbitrary entity expansion (CVE equivalent)
  Affected: >=0.8.0, <0.9.4
  Fix:      upgrade to 0.9.4

WARNING   log-formatter@0.9.3
  Advisory: THKT-2025-0061 — unbounded log line length (DoS, low severity)
  Affected: <1.0.0
  Fix:      upgrade to 1.0.0

2 advisories found (1 critical, 1 warning).
```

Exit code is non-zero when any advisory is found, making it CI-safe to gate on.

## Understand the severity levels

| Severity | Meaning |
|---|---|
| `CRITICAL` | Exploitable without preconditions; block release |
| `HIGH` | Significant risk; fix before next release |
| `WARNING` | Low-severity or DoS-only; address in next update cycle |
| `INFO` | Yanked version or deprecation notice |

## Apply fixes automatically

For advisories where a non-breaking fix is available within your existing version specifiers, use:

```bash
thicket audit --fix
```

Thicket resolves the smallest satisfying upgrade for each affected package and rewrites `thicket.lock`. It does not change `bramble.toml` unless the specifier itself blocks the fix.

Inspect what changed before committing:

```bash
thicket audit --fix --dry-run
```

```text
Would upgrade:
  bramble-xml  0.9.1  →  0.9.4
```

## Handle advisories that require specifier changes

If the fixed version falls outside your declared constraint, `thicket audit --fix` prints the affected package and exits without modifying anything:

```text
CRITICAL  bramble-xml@0.9.1
  Fix requires >=0.9.4, but bramble.toml specifies "~0.9.1" (< 0.9.2).
  Widen your constraint manually and re-run thicket audit --fix.
```

Edit `bramble.toml` to widen the constraint, then rerun:

```toml
[dependencies]
bramble-xml = "^0.9.4"    # was ~0.9.1
```

```bash
thicket install
thicket audit
```

## Ignore an advisory

If you have assessed an advisory and determined it does not apply to your usage, suppress it with an entry in `bramble.toml`:

```toml
[[audit.ignore]]
id      = "THKT-2025-0061"
reason  = "We never log user-controlled input; DoS vector does not apply."
expires = "2026-01-01"
```

Thicket records the `reason` and `expires` date. Expired ignores produce an `INFO` advisory reminding you to re-evaluate. See [pinning and updating versions](xref:bramble.how-to.thicket.pin-and-update-versions) for the update workflow once you decide to act.
