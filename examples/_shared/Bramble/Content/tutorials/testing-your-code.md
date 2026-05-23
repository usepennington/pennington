---
title: "Testing your code"
description: "Write unit tests with std/testing, organise them in a test module, and run them with bramble test."
uid: bramble.tutorials.testing-your-code
order: 90
sectionLabel: "Tutorials"
tags: [testing, unit-tests, bramble-test, assertions]
---

Bramble's test runner is built into the toolchain — no third-party library needed for basic unit testing. Tests live alongside your code in `test` blocks, or in dedicated `*_test.bramble` files. The `bramble test` command finds and runs them all.

## Your first test

Add a `test` block to any `.bramble` file. Each named test function inside is discovered automatically.

```bramble
import std/testing

fn add(a: i64, b: i64) -> i64 {
    a + b
}

test {
    fn test_add_positive() {
        testing.assert_eq(add(2, 3), 5)
    }

    fn test_add_negative() {
        testing.assert_eq(add(-4, 1), -3)
    }
}
```

Run the tests from the directory containing the file.

```bash
bramble test
```

```text
running 2 tests

  PASS  test_add_positive   (0.1ms)
  PASS  test_add_negative   (0.1ms)

2 passed, 0 failed
```

The `test` block and everything inside it is stripped from release builds — it contributes zero code to `bramble run --release`.

## Common assertion functions

`std/testing` provides a small, focused assertion API.

| Function | Checks |
|----------|--------|
| `assert(cond)` | `cond` is `true` |
| `assert_eq(a, b)` | `a == b` |
| `assert_ne(a, b)` | `a != b` |
| `assert_lt(a, b)` | `a < b` |
| `assert_err(result)` | `result` is `Err` |
| `assert_ok(result)` | `result` is `Ok` |
| `fail(msg)` | Always fails with message |

When an assertion fails, the runner prints the actual values, the file, and the line number — you rarely need to add extra context to the message.

## Separating tests into their own file

For larger modules, place tests in a companion file named `<module>_test.bramble`. The test runner finds any file matching `*_test.bramble` recursively.

```bash
math.bramble
math_test.bramble
```

```bramble
// math_test.bramble
import std/testing
import "math"   // relative import from the same directory

test {
    fn test_clamp_below_min() {
        testing.assert_eq(math.clamp(3.0, 5.0, 10.0), 5.0)
    }

    fn test_clamp_above_max() {
        testing.assert_eq(math.clamp(15.0, 5.0, 10.0), 10.0)
    }

    fn test_clamp_in_range() {
        testing.assert_eq(math.clamp(7.0, 5.0, 10.0), 7.0)
    }
}
```

## Testing Result-returning functions

`assert_ok` and `assert_err` make it straightforward to verify fallible functions without boilerplate unpacking.

```bramble
import std/testing
import std/fs

test {
    fn test_read_missing_file() {
        let result = fs.read_text("/no/such/file.txt")
        testing.assert_err(result)
    }
}
```

> [!TIP]
> Use `bramble test --filter test_clamp` to run only tests whose names contain a given substring. This speeds up iteration when you are focused on one area of the codebase.

With a test suite in place you can refactor and extend code confidently. Head to [Building a CLI tool](xref:bramble.tutorials.building-a-cli-tool) to put everything together in a real program.
