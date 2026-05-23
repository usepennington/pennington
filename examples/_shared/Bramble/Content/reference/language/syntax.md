---
title: "Syntax"
description: "Reference for Bramble's lexical structure, statement and expression forms, blocks, comments, and literals."
uid: bramble.reference.language.syntax
order: 110
sectionLabel: "Language"
tags: [syntax, lexical, expressions, literals, blocks]
---

Bramble source files are UTF-8 encoded text. This page describes the lexical structure, the distinction between statements and expressions, block forms, comment syntax, and the set of literal value forms.

## Lexical structure

Identifiers begin with a letter or underscore, followed by any combination of letters, digits, and underscores. Identifiers are case-sensitive. Whitespace (spaces, tabs, newlines) is insignificant except as a token separator. Bramble has no statement-terminating semicolons; newlines act as implicit boundaries where unambiguous.

```bramble
let count = 0
let _private = "hidden"
let camelCase = true
```

## Statements vs expressions

Every construct in Bramble is either a statement or an expression. Expressions produce values; statements do not (they produce `()`). The last expression in a block is that block's value. A `return` exits the enclosing function early.

```bramble
fn max(a: i64, b: i64) -> i64 {
    if a > b { a } else { b }   // block expression — no return needed
}
```

`let` bindings are statements. Assignments (`x = 5`) are also statements and produce `()`.

## Blocks

A block is a brace-enclosed sequence of statements and a final optional expression:

```bramble
let result = {
    let x = 10
    let y = 20
    x + y          // value of the block: 30
}
```

Blocks create their own scope. Bindings introduced inside a block are not visible outside it.

## Comments

| Form | Syntax | Notes |
|---|---|---|
| Line comment | `// text` | Extends to end of line |
| Block comment | `/* text */` | May span multiple lines; does not nest |
| Doc comment | `/// text` | Attached to the next declaration; Markdown-formatted |

```bramble
/// Returns the absolute value of `n`.
fn abs(n: i64) -> i64 {
    if n < 0 { -n } else { n }
}
```

## Literals

| Kind | Examples | Type inferred |
|---|---|---|
| Integer | `0`, `42`, `1_000_000` | `i64` |
| Float | `3.14`, `1.0e-4`, `0.5` | `f64` |
| Boolean | `true`, `false` | `bool` |
| Character | `'a'`, `'\n'`, `'\u{1F33F}'` | `char` |
| String | `"hello"`, `"line\n"` | `str` |
| Interpolated string | `"Hello, ${name}!"` | `str` |
| Unit | `()` | `()` |
| List | `[1, 2, 3]` | `[i64]` |
| Map | `{"a": 1, "b": 2}` | `{str: i64}` |
| Option some | `Some(42)` | `Option<i64>` |
| Option none | `None` | `Option<T>` |

Numeric literals accept underscores as digit separators anywhere after the first digit. Hex literals use the `0x` prefix; binary literals use `0b`.

See [operators](xref:bramble.reference.language.operators) for expressions built from literals, and [types](xref:bramble.reference.language.types) for the full type descriptions.
