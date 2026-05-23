---
title: "Configure formatting"
description: "Adjust Sprig's formatter to match your project's style by editing .sprig.toml and running sprig fmt."
uid: bramble.how-to.sprig.configure-formatting
order: 410
sectionLabel: "Sprig"
tags: [formatting, sprig, style, config]
---

Sprig ships with opinionated defaults, but every project has its own conventions. A `.sprig.toml` file at the project root lets you tune indentation, line width, and quote style without changing any source code by hand.

## Create or locate .sprig.toml

If you ran `bramble new` to scaffold the project, a `.sprig.toml` is already present. Otherwise, create one at the root (next to `bramble.toml`):

```bash
touch .sprig.toml
```

Sprig reads the nearest `.sprig.toml` walking up from the file being formatted, then merges with any workspace-level config.

## Set indentation

The `indent` key accepts `"spaces"` or `"tabs"`, with `width` controlling the column count for spaces:

```toml
[fmt]
indent = "spaces"
width = 4
```

Setting `indent = "tabs"` ignores `width`. The default is 2-space indentation.

## Set line width

`line_width` is the soft wrap target for expressions and argument lists. Sprig will not break string literals to meet it.

```toml
[fmt]
line_width = 100
```

Values between 80 and 120 are the most common. The default is 88.

## Set quote style

Bramble string literals can use single or double quotes interchangeably. Sprig normalises them to whichever style you prefer:

```toml
[fmt]
quotes = "double"
```

Accepted values: `"double"` (default) or `"single"`. Template strings (`"${x}"`) are always double-quoted regardless of this setting.

## Run the formatter

Format all Bramble files in the project:

```bash
sprig fmt
```

Pass a path to format a single file or directory:

```bash
sprig fmt src/lib/parser.brm
```

## Check formatting in CI

The `--check` flag exits with a non-zero status if any file would change, without writing anything. Use it in a CI step to enforce style:

```bash
sprig fmt --check
```

> [!NOTE]
> `--check` prints a diff to stderr for each file that would be reformatted, making it straightforward to identify what needs fixing without running the full formatter.

For a complete list of `.sprig.toml` keys, see the [Sprig configuration reference](xref:bramble.reference.config.sprig-config). To lint for code-quality issues in the same pass, see [Write a custom lint](xref:bramble.how-to.sprig.write-a-custom-lint).
