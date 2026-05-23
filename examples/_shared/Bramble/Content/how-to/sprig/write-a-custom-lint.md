---
title: "Write a custom lint"
description: "Define a project-specific lint rule that walks the Bramble AST and register it so sprig lint can enforce it."
uid: bramble.how-to.sprig.write-a-custom-lint
order: 420
sectionLabel: "Sprig"
tags: [sprig, lint, custom-rule, ast, code-quality]
---

Sprig's built-in rules cover general Bramble idioms, but teams often need rules specific to their codebase — naming conventions, forbidden APIs, required annotations. Custom lint rules are Bramble scripts that receive an AST view and emit diagnostics.

## Project layout

Custom rules live under a directory you declare in `.sprig.toml`. A common convention:

```text
my-project/
  .sprig.toml
  lint/
    no_panic_unwrap.brm
```

## Declare the lint directory

Tell Sprig where to look for rule scripts:

```toml
[lint]
rules_dir = "lint"
```

Every `.brm` file in `rules_dir` is loaded as a rule. The filename (minus extension) becomes the rule code prefix; the rule itself declares its full code.

## Write the rule script

A lint rule exports a `check` function that accepts a `Node` and returns `Diagnostic[]`. Use the `std/lint` module for the diagnostic builder and AST node types.

```bramble
import std/lint { Diagnostic, Node, NodeKind }

-- Rule S201: do not call .unwrap() directly on a Result.
-- Prefer propagation with ? or an explicit match.
export fn check(node: Node) -> Diagnostic[] {
    if node.kind != NodeKind.MethodCall {
        return []
    }

    let method_name = node.method_name() ?? return []

    if method_name != "unwrap" {
        return []
    }

    let receiver_type = node.receiver().inferred_type() ?? return []

    if !receiver_type.name.starts_with("Result") {
        return []
    }

    [Diagnostic.new(
        code: "S201",
        message: "Avoid .unwrap() on Result; use ? or match instead",
        span: node.span(),
        severity: .Warning,
    )]
}
```

The `??` operator short-circuits on `Option.None`, keeping the rule concise.

## Understand the rule contract

| Export | Type | Required | Purpose |
|--------|------|----------|---------|
| `check` | `fn(Node) -> Diagnostic[]` | Yes | Called for every node in the file's AST |
| `description` | `String` | No | Shown in `sprig lint --list` output |
| `severity` | `Severity` | No | Default severity if not set per diagnostic |

> [!NOTE]
> `check` is called once per node. Sprig walks the full AST depth-first; you do not need to recurse manually.

## Run the rule

```bash
sprig lint
```

To run only your custom rules:

```bash
sprig lint --rules S201
```

Violations appear inline with file, line, column, and the diagnostic message. Exit code is non-zero when any warning or error is emitted.

For suppressing a specific warning on a line where it is expected, see [Suppress a lint warning](xref:bramble.how-to.sprig.suppress-a-lint-warning). The full set of `.sprig.toml` lint options is in the [Sprig configuration reference](xref:bramble.reference.config.sprig-config).
