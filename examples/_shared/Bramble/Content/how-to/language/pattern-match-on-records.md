---
title: "Pattern match on records"
description: "Destructure record fields, apply guards, and use wildcards inside match expressions."
uid: bramble.how-to.language.pattern-match-on-records
order: 130
sectionLabel: "Language"
tags: [pattern-matching, records, destructuring, match]
---

Bramble's `match` expression works on record shapes as well as union variants. You can pull fields out directly in the pattern rather than accessing them field by field after the match.

## Destructure fields in a pattern

List the fields you care about by name inside `{ }`. Bound names are available in the arm body.

```bramble
record Point { x: float, y: float }

fn describe(p: Point) -> str {
    match p {
        { x: 0.0, y: 0.0 } => "origin",
        { x, y: 0.0 }      => "on x-axis at ${x}",
        { x: 0.0, y }      => "on y-axis at ${y}",
        { x, y }           => "(${x}, ${y})",
    }
}
```

When the binding name matches the field name you can write the field once, as with `x` and `y` in the final arm.

## Use guards for conditional logic

A `if` guard after the pattern applies an extra condition. The arm only matches when the guard expression is true.

```bramble
record Order { status: str, total: float }

fn classify(order: Order) -> str {
    match order {
        { status: "paid", total } if total >= 1000.0 => "high-value",
        { status: "paid", .. }                       => "standard",
        { status: "pending", .. }                    => "awaiting payment",
        { status, .. }                               => "unknown status: ${status}",
    }
}
```

The `..` wildcard ignores remaining fields you do not need in that arm.

## Match nested records

Patterns nest. If a field is itself a record, match its shape inline.

```bramble
record Address { city: str, country: str }
record Customer { name: str, address: Address }

fn greet(c: Customer) -> str {
    match c {
        { name, address: { country: "NZ", city } } =>
            "Kia ora ${name}, from ${city}",
        { name, address: { city } } =>
            "Hello ${name}, from ${city}",
    }
}
```

## Bind a field and test its value

You can bind a field to a name in one arm while testing a literal in another. The compiler tracks that every case is covered.

```bramble
record Response { code: int, body: str }

fn handle(resp: Response) {
    match resp {
        { code: 200, body } => process(body),
        { code: 404, .. }   => io::eprintln("not found"),
        { code, .. }        => io::eprintln("unexpected code ${code}"),
    }
}
```

> [!TIP]
> If the compiler reports a non-exhaustive match, add a catch-all arm using `{ .. }` as the final pattern. This catches any record shape not handled above.

For a broader view of pattern matching across union types and literals, see the [syntax reference](xref:bramble.reference.language.syntax). Record destructuring also appears throughout the [control flow tutorial](xref:bramble.tutorials.control-flow).
