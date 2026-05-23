---
title: "Suppress a lint warning"
description: "Silence a specific Sprig lint warning on a single line or across a file pattern using inline attributes and .sprig.toml ignore globs."
uid: bramble.how-to.sprig.suppress-a-lint-warning
order: 430
sectionLabel: "Sprig"
tags: [sprig, lint, suppress, allow, ignore]
---

Suppression is a deliberate escape hatch, not a convenience. Use it when a lint rule fires on code that is intentionally exceptional — generated code, test scaffolding, or a known false positive — and document why with a comment.

## Suppress a single line with an inline attribute

Place an `#[allow(...)]` attribute on the line immediately before the statement that triggers the warning. The attribute applies only to that statement.

```bramble
import std/ffi { unsafe_cast }

fn coerce_buffer(raw: Bytes) -> String {
    -- FFI boundary: the host guarantees this buffer is valid UTF-8.
    #[allow(S044)]
    let s = unsafe_cast::<String>(raw)
    s
}
```

Multiple codes can be grouped in one attribute:

```bramble
#[allow(S044, S201)]
let result = legacy_api.call().unwrap()
```

## Suppress a block of statements

Wrap a range of statements with matching `#[allow(...)]` / `#[end_allow(...)]` markers:

```bramble
#[allow(S102)]
let a = old_compat_fn(x)
let b = old_compat_fn(y)
let c = old_compat_fn(z)
#[end_allow(S102)]
```

Block suppression is intentionally verbose. If you find yourself suppressing large sections, reconsider whether the rule is misconfigured rather than whether the code is correct.

> [!NOTE]
> Block markers must use matching codes. `#[allow(S102)]` paired with `#[end_allow(S044)]` is a Sprig parse error (code `B0312`).

## Suppress across files with .sprig.toml ignore globs

For generated files or third-party vendored code that you do not own, configure ignore globs in `.sprig.toml` rather than sprinkling suppressions throughout:

```toml
[lint]
ignore = [
    "vendor/**",
    "generated/**/*.brm",
    "tests/fixtures/bad_*.brm",
]
```

Globs are matched relative to the project root. Files matching any glob are skipped entirely — both linting and formatting.

## Check which rules are suppressed

To audit suppressions in the current project:

```bash
sprig lint --list-suppressions
```

This prints every active inline `#[allow]` and the glob patterns from `.sprig.toml`, so you can identify suppressions that are no longer needed after a rule change.

For background on the full set of built-in codes, see the [Sprig CLI reference](xref:bramble.reference.cli.sprig). To write a rule that others on your team might want to suppress, see [Write a custom lint](xref:bramble.how-to.sprig.write-a-custom-lint).
