---
title: "Format and interpolate strings"
description: "Embed expressions in strings with ${}, control width and precision with format specifiers, and build formatted output with format()."
uid: bramble.how-to.language.format-and-interpolate-strings
order: 170
sectionLabel: "Language"
tags: [strings, interpolation, formatting, stdlib]
---

Bramble string literals support inline expression interpolation with `${}`. For more precise control over width, padding, and numeric precision, the `std/strings` module provides a `format()` function with a specifier mini-language.

## Embed expressions with ${}

Any expression inside `${}` is evaluated and converted to a string via its `to_str()` implementation.

```bramble
let name = "world"
let count = 42
io::println("Hello, ${name}! Items: ${count}")
// Hello, world! Items: 42
```

Expressions can be arbitrarily complex — method calls, arithmetic, ternary-style `if` expressions — though long expressions inside `${}` hurt readability.

```bramble
let label = "result: ${if value >= 0 { "+" } else { "" }}${value}"
```

## Control width and alignment

`format()` takes a format string where `{}` is a plain placeholder and `{:...}` carries a specifier.

```bramble tabs=true title="Left align"
import std/strings

let s = strings::format("{:<10} | {:>10}", ["left", "right"])
// "left       |      right"
```

```bramble tabs=true title="Zero-pad numbers"
import std/strings

let s = strings::format("{:08}", [255])
// "00000255"
```

The specifier syntax is `{:[fill][align][width][.precision][type]}`. Fill defaults to a space; align is `<` (left), `>` (right), or `^` (center).

## Set numeric precision

Use `.N` to fix decimal places on floats.

```bramble
import std/strings

let pi = 3.14159265358979
io::println(strings::format("pi ≈ {:.4}", [pi]))
// pi ≈ 3.1416
```

## Format integers in different bases

The type suffix `b`, `o`, and `x` produce binary, octal, and lowercase hexadecimal output respectively. `X` gives uppercase hex.

```bramble
import std/strings

let n = 255
io::println(strings::format("dec={} hex={:x} oct={:o} bin={:b}", [n, n, n, n]))
// dec=255 hex=ff oct=377 bin=11111111
```

## Build multiline output with a string builder

When constructing a large string in a loop, appending to a `StringBuilder` is more efficient than repeated `+` concatenation.

```bramble
import std/strings

let mut sb = strings::builder()
for item in items {
    sb.push("  - ${item.name}: ${item.value}\n")
}
let report = sb.to_str()
```

> [!TIP]
> Interpolation (`${}`) is syntax sugar resolved at compile time. `format()` is a runtime function that parses the format string. Prefer `${}` for simple cases and `format()` when you need width, precision, or base control.

The full specifier grammar and all conversion types are listed in the [strings standard library reference](xref:bramble.reference.stdlib.strings). For building structured output in a CLI tool, see [parse command-line arguments](xref:bramble.how-to.language.parse-command-line-args).
