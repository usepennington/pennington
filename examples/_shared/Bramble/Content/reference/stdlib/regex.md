---
title: "std/regex"
description: "Reference for the std/regex module, covering pattern compilation, matching, capture groups, and string replacement."
uid: bramble.reference.stdlib.regex
order: 300
sectionLabel: "Standard library"
tags: [stdlib, regex, patterns, strings]
---

The `std/regex` module provides regular-expression support using a compiled `Pattern` type. Patterns are compiled once and reused, keeping repeated matching efficient. The regex dialect is a strict subset of PCRE2 — lookaheads are supported, backreferences are not.

## Importing

```bramble
import std/regex
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `compile` | `(pattern: str) -> Result<Pattern, RegexError>` | Compiles a regex pattern string into a reusable `Pattern`. |
| `is_match` | `(p: Pattern, input: str) -> bool` | Returns `true` if the pattern matches anywhere in `input`. |
| `find` | `(p: Pattern, input: str) -> Option<Match>` | Returns the first match in `input`, or `Option.None`. |
| `find_all` | `(p: Pattern, input: str) -> [Match]` | Returns all non-overlapping matches in `input`. |
| `captures` | `(p: Pattern, input: str) -> Option<Captures>` | Returns named and positional capture groups for the first match. |
| `replace` | `(p: Pattern, input: str, with: str) -> str` | Replaces the first match in `input` with `with`. |
| `replace_all` | `(p: Pattern, input: str, with: str) -> str` | Replaces all non-overlapping matches in `input` with `with`. |
| `Match.text` | `(self: Match) -> str` | Returns the matched substring. |
| `Match.start` | `(self: Match) -> i64` | Returns the byte offset of the match start. |
| `Captures.get` | `(self: Captures, name: str) -> Option<str>` | Returns the value of a named capture group. |
| `Captures.at` | `(self: Captures, index: i64) -> Option<str>` | Returns the value of a positional capture group. |

## Replacement strings

In `replace` and `replace_all`, the `with` argument may reference capture groups using `$1`, `$2` (positional) or `$name` (named). A literal `$` is written as `$$`.

## Example: extracting a version number

```bramble
import std/regex

let pat = regex.compile(r"v(?P<major>\d+)\.(?P<minor>\d+)")?

let input = "Released in v1.2 of the toolchain."

match regex.captures(pat, input) {
    Option.Some(caps) => {
        let major = caps.get("major").unwrap()
        let minor = caps.get("minor").unwrap()
        println("major=${major} minor=${minor}")
    }
    Option.None => println("no version found")
}
```

```text
major=1 minor=2
```

## Example: replace all whitespace runs

```bramble
import std/regex

let ws = regex.compile(r"\s+")?
let cleaned = regex.replace_all(ws, "foo   bar\tbaz", " ")
println(cleaned)
```

```text
foo bar baz
```

## Related

- [std/io](xref:bramble.reference.stdlib.io) — read strings from files before matching
- [Handle errors with Result](xref:bramble.how-to.language.handle-errors-with-result) — handling `compile` failures
