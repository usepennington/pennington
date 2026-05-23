---
title: "sprig"
description: "Reference for the sprig CLI, Bramble's source formatter and linter."
uid: bramble.reference.cli.sprig
order: 440
sectionLabel: "CLI"
tags: [cli, sprig, formatting, linting, style]
---

`sprig` enforces consistent style and catches common issues in Bramble source files. It covers formatting (`fmt`), linting (`lint`), and a combined check mode suitable for CI (`check`).

## Subcommands

| Subcommand | Purpose |
|---|---|
| `fmt` | Reformat source files in-place according to the canonical style |
| `lint` | Analyse source files and report rule violations |
| `check` | Run both `fmt` and `lint` in read-only mode; exit non-zero on any finding |

## Global flags

| Flag | Default | Description |
|---|---|---|
| `--config <path>` | `.sprig.toml` | Override the configuration file path |
| `--quiet` / `-q` | false | Suppress per-file progress; print only a summary |
| `--color` | `auto` | Colour output: `always`, `never`, or `auto` |

## sprig fmt

Rewrites files in-place. Accepts a list of paths; defaults to all `.br` files under the current directory.

```bash
sprig fmt
sprig fmt src/parser.br src/lexer.br
sprig fmt --check   # report diffs without writing; exit non-zero if any file would change
```

The `--check` flag makes `fmt` read-only, which is useful in pre-commit hooks and CI pipelines.

## sprig lint

Reports rule violations with file, line, column, rule code, and a short message.

```bash
sprig lint
sprig lint src/
sprig lint --fix         # auto-fix violations that have a safe mechanical correction
sprig lint --rule W0031  # run only one rule
```

```text
src/parser.br:14:5  W0012  unused variable `tok`
src/lexer.br:82:1   E0041  shadowed binding in inner scope
src/lexer.br:103:9  W0031  prefer `Option` over bare boolean return
3 violations (1 error, 2 warnings)
```

| Flag | Description |
|---|---|
| `--fix` | Apply safe automatic fixes where available |
| `--rule <code>` | Run only the named rule; can be repeated |
| `--deny <code>` | Treat a warning-level rule as an error |
| `--allow <code>` | Suppress a rule entirely for this invocation |

## sprig check

Runs `fmt --check` and `lint` together, exiting non-zero if either reports findings. Use this as the single CI gate command.

```bash
sprig check
sprig check src/ tests/
```

```text
[fmt] 0 files would change
[lint] src/lexer.br:82:1  E0041  shadowed binding in inner scope
check failed: 1 lint error
```

## Exit codes

| Code | Meaning |
|---|---|
| 0 | No findings (or `fmt` made all required changes) |
| 1 | One or more lint errors or formatting differences found |
| 2 | Internal tool error (bad arguments, missing config, I/O failure) |

See [.sprig.toml](xref:bramble.reference.config.sprig-config) for rule toggles and ignore globs, and [configure formatting](xref:bramble.how-to.sprig.configure-formatting) for a workflow walkthrough.
