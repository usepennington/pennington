---
title: ".sprig.toml"
description: "Reference for the .sprig.toml configuration file, which controls sprig's formatting style, lint rules, and ignore patterns."
uid: bramble.reference.config.sprig-config
order: 530
sectionLabel: "Configuration"
tags: [configuration, sprig, formatting, linting, style]
---

`.sprig.toml` configures the `sprig` formatter and linter. Place it at the project root; `sprig` searches upward from the target file path to find it. All keys are optional — omitting a section applies the built-in defaults.

## [format]

Controls code style applied by `sprig fmt`.

| Key | Type | Default | Description |
|---|---|---|---|
| `indent` | string | `"spaces"` | Indentation style: `"spaces"` or `"tabs"` |
| `indent_width` | integer | `4` | Number of spaces per indent level (ignored when `indent = "tabs"`) |
| `max_line_width` | integer | `100` | Soft line-length limit; formatter wraps where possible |
| `trailing_newline` | bool | `true` | Ensure files end with a single newline |
| `import_order` | string | `"stdlib-first"` | Import grouping: `"stdlib-first"`, `"alpha"`, or `"preserve"` |
| `chain_style` | string | `"trailing-dot"` | Method-chain line break style: `"trailing-dot"` or `"leading-dot"` |

## [lint]

Controls which lint rules run and at what severity.

| Key | Type | Default | Description |
|---|---|---|---|
| `rules` | map of string → string | all enabled | Override rule severity: `"error"`, `"warn"`, or `"off"` |
| `deny_warnings` | bool | `false` | Treat all warnings as errors (equivalent to `--deny all`) |
| `max_complexity` | integer | `20` | Cyclomatic complexity threshold for rule `C0001` |
| `max_fn_lines` | integer | `80` | Line-count threshold for rule `C0002` |

### Rule codes

Rules follow the pattern `[EWCW]NNNN`. The prefix indicates default severity: `E` = error, `W` = warning, `C` = complexity.

| Code | Default | Description |
|---|---|---|
| `E0041` | error | Shadowed binding in inner scope |
| `W0012` | warn | Unused variable |
| `W0031` | warn | Prefer `Option` over bare boolean sentinel return |
| `W0055` | warn | Public function missing doc comment |
| `C0001` | warn | Function exceeds cyclomatic complexity threshold |
| `C0002` | warn | Function body exceeds line-count threshold |

## [lint.ignore]

Lists glob patterns for files and directories that `sprig lint` and `sprig fmt` skip entirely.

| Key | Type | Description |
|---|---|---|
| `paths` | array of strings | Glob patterns relative to the project root |

## Sample .sprig.toml

```toml
[format]
indent        = "spaces"
indent_width  = 2
max_line_width = 120
import_order  = "stdlib-first"
chain_style   = "leading-dot"

[lint]
deny_warnings  = false
max_complexity = 15
max_fn_lines   = 60

[lint.rules]
W0055 = "off"    # public doc comment not required in this project
C0002 = "error"  # treat long functions as hard errors

[lint.ignore]
paths = [
  "src/generated/**",
  "vendor/**",
  "tests/fixtures/**",
]
```

See the [`sprig` CLI reference](xref:bramble.reference.cli.sprig) for per-invocation flags and [configure formatting](xref:bramble.how-to.sprig.configure-formatting) for a setup walkthrough.
