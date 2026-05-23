---
title: "std/testing"
description: "Reference for the std/testing module, covering the test attribute, assertion functions, and how to run tests with the Bramble CLI."
uid: bramble.reference.stdlib.testing
order: 290
sectionLabel: "Standard library"
tags: [stdlib, testing, assertions, tdd]
---

The `std/testing` module is Bramble's built-in test harness. Tests are ordinary functions annotated with `@test`, and the test runner discovers and executes them automatically when you invoke `bramble test`.

## Importing

```bramble
import std/testing
```

## Function reference

| Function | Signature | Description |
|---|---|---|
| `assert` | `(cond: bool, msg: Option<str>) -> ()` | Fails the test if `cond` is `false`, printing `msg` when provided. |
| `assert_eq` | `(left: T, right: T, msg: Option<str>) -> ()` | Fails the test if `left != right`, showing both values in the output. |
| `assert_ne` | `(left: T, right: T, msg: Option<str>) -> ()` | Fails the test if `left == right`. |
| `assert_err` | `(r: Result<T, E>) -> E` | Fails the test if `r` is `Ok`; returns the inner error value if it is `Err`. |
| `assert_ok` | `(r: Result<T, E>) -> T` | Fails the test if `r` is `Err`; returns the inner value if it is `Ok`. |
| `fail` | `(msg: str) -> ()` | Unconditionally fails the test with the given message. |

## The `@test` attribute

Decorate any top-level function that takes no parameters with `@test` to mark it as a test case. The test runner collects all such functions from files matching `*_test.brb` or any file when run with `bramble test --all`.

```bramble
import std/testing

@test
fn addition_is_commutative() {
    testing.assert_eq(1 + 2, 2 + 1, Option.None)
}
```

## Running tests

```text
bramble test               # runs tests in *_test.brb files
bramble test --all         # runs tests in all .brb files
bramble test --filter add  # runs only tests whose name contains "add"
```

Test output uses a compact TAP-like format. Failures print the file name, line number, and both sides of a failed `assert_eq`.

## Example: testing a Result-returning function

```bramble
import std/testing

fn divide(a: f64, b: f64) -> Result<f64, str> {
    if b == 0.0 {
        return Result.Err("division by zero")
    }
    Result.Ok(a / b)
}

@test
fn divide_returns_quotient() {
    let v = testing.assert_ok(divide(10.0, 4.0))
    testing.assert_eq(v, 2.5, Option.None)
}

@test
fn divide_by_zero_is_err() {
    let msg = testing.assert_err(divide(1.0, 0.0))
    testing.assert_eq(msg, "division by zero", Option.None)
}
```

```text
ok  divide_returns_quotient
ok  divide_by_zero_is_err
2 passed, 0 failed
```

> [!WARNING]
> `@test` functions run in the default sandbox, so network and filesystem calls require the same capability flags as production code. Pass `--allow-net` or `--allow-fs` to `bramble test` when your tests need those capabilities.

## Related

- [Testing your code tutorial](xref:bramble.tutorials.testing-your-code)
- [Handle errors with Result](xref:bramble.how-to.language.handle-errors-with-result)
