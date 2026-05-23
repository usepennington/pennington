---
title: "std/strings"
description: "Reference for the std/strings module, covering string manipulation, inspection, and transformation functions."
uid: bramble.reference.stdlib.strings
order: 210
sectionLabel: "Standard library"
tags: [stdlib, strings, text]
---

The `std/strings` module provides functions for working with `str` values: splitting, joining, searching, transforming case, and iterating over characters. All functions treat strings as UTF-8 sequences.

## Importing

```bramble
import std/strings
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `split` | `(s: str, sep: str) -> [str]` | Splits `s` on every occurrence of `sep` and returns the resulting list. |
| `join` | `(parts: [str], sep: str) -> str` | Concatenates `parts` into a single string, inserting `sep` between each element. |
| `trim` | `(s: str) -> str` | Removes leading and trailing ASCII whitespace from `s`. |
| `trim_prefix` | `(s: str, prefix: str) -> str` | Removes `prefix` from the start of `s` if present; otherwise returns `s` unchanged. |
| `trim_suffix` | `(s: str, suffix: str) -> str` | Removes `suffix` from the end of `s` if present; otherwise returns `s` unchanged. |
| `replace` | `(s: str, from: str, to: str) -> str` | Replaces every non-overlapping occurrence of `from` in `s` with `to`. |
| `replace_n` | `(s: str, from: str, to: str, n: i64) -> str` | Replaces up to `n` occurrences of `from` in `s` with `to`. |
| `to_upper` | `(s: str) -> str` | Returns `s` with all ASCII letters converted to uppercase. |
| `to_lower` | `(s: str) -> str` | Returns `s` with all ASCII letters converted to lowercase. |
| `contains` | `(s: str, sub: str) -> bool` | Returns `true` if `sub` appears anywhere in `s`. |
| `starts_with` | `(s: str, prefix: str) -> bool` | Returns `true` if `s` begins with `prefix`. |
| `ends_with` | `(s: str, suffix: str) -> bool` | Returns `true` if `s` ends with `suffix`. |
| `len` | `(s: str) -> i64` | Returns the number of UTF-8 bytes in `s`. |
| `char_count` | `(s: str) -> i64` | Returns the number of Unicode scalar values (characters) in `s`. |
| `chars` | `(s: str) -> [char]` | Returns a list of every `char` in `s` in order. |
| `repeat` | `(s: str, n: i64) -> str` | Concatenates `s` with itself `n` times. |
| `index_of` | `(s: str, sub: str) -> Option<i64>` | Returns the byte offset of the first occurrence of `sub` in `s`, or `None`. |

## Example

Parse a comma-separated tag string, normalize each tag, and reassemble it.

```bramble
import std/strings

fn normalize_tags(raw: str) -> str {
    let parts = strings.split(raw, ",")
    let mut cleaned: [str] = []
    for part in parts {
        let tag = strings.to_lower(strings.trim(part))
        if strings.len(tag) > 0 {
            cleaned.push(tag)
        }
    }
    strings.join(cleaned, ", ")
}

let result = normalize_tags("  Bramble , LANG , scripting  ")
std/io.println(result)
```

```text
bramble, lang, scripting
```

## Notes on `len` vs `char_count`

`len` counts bytes, not characters. For ASCII-only strings the two values are equal. For strings containing multi-byte characters — such as accented letters or emoji — `char_count` gives the human-visible length.

```bramble
import std/strings

let s = "café"
strings.len(s)        // 5 (bytes)
strings.char_count(s) // 4 (characters)
```

> [!WARNING]
> Slicing a `str` by raw byte index can split a multi-byte sequence and produce invalid UTF-8. Use `chars` to work with individual characters safely.

## Related

- [String formatting and interpolation](xref:bramble.how-to.language.format-and-interpolate-strings)
- [Type reference](xref:bramble.reference.language.types)
- [std/regex](xref:bramble.reference.stdlib.regex)
