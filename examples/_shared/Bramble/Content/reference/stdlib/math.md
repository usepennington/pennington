---
title: "std/math"
description: "Reference for the std/math module, covering arithmetic, rounding, exponentiation, and numeric constants."
uid: bramble.reference.stdlib.math
order: 230
sectionLabel: "Standard library"
tags: [stdlib, math, numeric]
---

The `std/math` module provides mathematical functions and constants for `i64` and `f64` values. Functions that are meaningful only for floating-point inputs accept `f64`; functions that work on both types have overloaded forms.

## Importing

```bramble
import std/math
```

## Constants

| Constant | Type | Value |
|---|---|---|
| `math.PI` | `f64` | 3.141592653589793 |
| `math.E` | `f64` | 2.718281828459045 |
| `math.TAU` | `f64` | 6.283185307179586 (2π) |
| `math.INF` | `f64` | Positive infinity |
| `math.NAN` | `f64` | Not-a-number sentinel |

## Function reference

| Function | Signature | Description |
|---|---|---|
| `abs` | `(x: f64) -> f64` | Returns the absolute value of `x`. |
| `abs_i` | `(x: i64) -> i64` | Returns the absolute value of an integer `x`. |
| `sqrt` | `(x: f64) -> f64` | Returns the non-negative square root of `x`. |
| `cbrt` | `(x: f64) -> f64` | Returns the cube root of `x`. |
| `pow` | `(base: f64, exp: f64) -> f64` | Returns `base` raised to the power `exp`. |
| `exp` | `(x: f64) -> f64` | Returns `e` raised to the power `x`. |
| `ln` | `(x: f64) -> f64` | Returns the natural logarithm of `x`. |
| `log` | `(x: f64, base: f64) -> f64` | Returns the logarithm of `x` with the given `base`. |
| `floor` | `(x: f64) -> f64` | Rounds `x` toward negative infinity. |
| `ceil` | `(x: f64) -> f64` | Rounds `x` toward positive infinity. |
| `round` | `(x: f64) -> f64` | Rounds `x` to the nearest integer, half away from zero. |
| `trunc` | `(x: f64) -> f64` | Truncates the fractional part of `x` toward zero. |
| `min` | `(a: f64, b: f64) -> f64` | Returns the smaller of `a` and `b`. |
| `max` | `(a: f64, b: f64) -> f64` | Returns the larger of `a` and `b`. |
| `min_i` | `(a: i64, b: i64) -> i64` | Returns the smaller of two integers. |
| `max_i` | `(a: i64, b: i64) -> i64` | Returns the larger of two integers. |
| `clamp` | `(x: f64, lo: f64, hi: f64) -> f64` | Constrains `x` to the range `[lo, hi]`. |
| `clamp_i` | `(x: i64, lo: i64, hi: i64) -> i64` | Constrains an integer to the range `[lo, hi]`. |
| `sin` | `(x: f64) -> f64` | Returns the sine of `x` in radians. |
| `cos` | `(x: f64) -> f64` | Returns the cosine of `x` in radians. |
| `tan` | `(x: f64) -> f64` | Returns the tangent of `x` in radians. |
| `is_nan` | `(x: f64) -> bool` | Returns `true` if `x` is `NAN`. |
| `is_inf` | `(x: f64) -> bool` | Returns `true` if `x` is positive or negative infinity. |

## Example

Compute the length of the hypotenuse and clamp a score to a valid range.

```bramble
import std/math

fn hypotenuse(a: f64, b: f64) -> f64 {
    math.sqrt(math.pow(a, 2.0) + math.pow(b, 2.0))
}

let h = hypotenuse(3.0, 4.0)
std/io.println("hypotenuse: ${h}")

let raw_score: f64 = 112.5
let score = math.clamp(raw_score, 0.0, 100.0)
std/io.println("clamped: ${score}")
```

```text
hypotenuse: 5.0
clamped: 100.0
```

## Integer vs floating-point variants

Functions suffixed with `_i` (`abs_i`, `min_i`, `max_i`, `clamp_i`) accept and return `i64`. The unsuffixed forms always operate on `f64`. Passing an integer literal to an unsuffixed function implicitly widens it; passing a `f64` to an `_i` form is a compile error.

> [!WARNING]
> `sqrt` of a negative number returns `NAN`, not an error. Check `is_nan` if your input may be negative, or validate it beforehand.

## Related

- [Type reference](xref:bramble.reference.language.types)
- [std/strings](xref:bramble.reference.stdlib.strings)
