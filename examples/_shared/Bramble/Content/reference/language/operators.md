---
title: "Operators and precedence"
description: "Reference table of Bramble operators ordered from highest to lowest precedence, with associativity and descriptions."
uid: bramble.reference.language.operators
order: 130
sectionLabel: "Language"
tags: [operators, precedence, arithmetic, expressions]
---

Bramble operators are listed below from highest to lowest precedence. Where multiple operators share a precedence level, the associativity column determines grouping. Unary operators bind tighter than any binary operator.

## Precedence table

| Precedence | Operator(s) | Associativity | Description |
|---|---|---|---|
| 14 (highest) | `-x`, `!x` | Right (unary) | Arithmetic negation; logical NOT |
| 13 | `as` | Left | Type cast: `x as f64` |
| 12 | `*`, `/`, `%` | Left | Multiplication, division, remainder |
| 11 | `+`, `-` | Left | Addition, subtraction |
| 10 | `..`, `..=` | None | Exclusive and inclusive range construction |
| 9 | `<<`, `>>` | Left | Bitwise left shift, right shift |
| 8 | `&` | Left | Bitwise AND |
| 7 | `^` | Left | Bitwise XOR |
| 6 | `\|` | Left | Bitwise OR |
| 5 | `==`, `!=`, `<`, `>`, `<=`, `>=` | None | Equality and relational comparison; non-associative (chaining requires explicit grouping) |
| 4 | `&&` | Left | Logical AND (short-circuits) |
| 3 | `\|\|` | Left | Logical OR (short-circuits) |
| 2 | `?` | Postfix | Early-return on `Err` or `None`; propagates to the caller's return type |
| 1 (lowest) | `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `\|=`, `^=` | Right | Assignment and compound assignment |

## Notes

### Range operators

`..` produces a half-open range `[start, end)`. `..=` produces a closed range `[start, end]`. Ranges are lazy — they do not allocate a list.

```bramble
for i in 0..10 {       // 0 through 9
    // ...
}

for i in 1..=10 {      // 1 through 10
    // ...
}
```

### The `?` operator

`?` is postfix and applies only inside functions that return `Result<T, E>` or `Option<T>`. On `Ok(v)` or `Some(v)` it unwraps to `v`; on `Err(e)` or `None` it returns immediately from the enclosing function with that error or `None`.

```bramble
fn read_number(path: str) -> Result<i64, str> {
    let text = std/fs.read_string(path)?   // propagates Err
    let n = std/conv.parse_i64(text)?      // propagates Err
    Ok(n)
}
```

### Comparison chaining

Comparisons are non-associative. `a < b < c` is a compile error ([B0005](xref:bramble.reference.language.error-codes)). Write `a < b && b < c` instead.

> **Caution:** `==` on `f64` values compares by bit-pattern. Floating-point rounding can cause two mathematically equal expressions to compare unequal. Use an epsilon comparison for approximate equality.
