---
title: "Why Bramble has no null"
description: "The reasoning behind Bramble's decision to replace null with Option, and what that costs and gains in practice."
uid: bramble.explanation.why-no-null
order: 20
sectionLabel: "Explanation"
tags: [null, option, safety, errors, types]
---

Null references have been called a billion-dollar mistake by the researcher who introduced them into a commercial language decades ago. Bramble takes that assessment seriously enough to make the absence of null a core language property rather than a linting rule.

## What null costs

The problem with null is not that the concept of "no value" is wrong—absence is a real and useful idea. The problem is that null is an invisible inhabitant of every reference type. When a variable is typed as `String`, null is also a valid value, but the type signature does not say so. The programmer has to remember which functions can return null, carry that knowledge in working memory, and check at the right moment. When they forget, the runtime raises an exception at the worst possible time.

This is fundamentally a type system failure. The type says one thing; the actual behavior is different.

## Option as an explicit contract

Bramble uses `Option[T]`, a sum type with two variants: `Some(value)` and `None`. A function that might not return a value says so in its return type. A function that returns `String` will always return a string—the type is not lying.

```bramble
fn find_user(id: UserId) -> Option[User] {
    // returns Some(user) or None
}

let result = find_user(requested_id)
match result {
    Some(user) => greet(user)
    None       => log.warn("user not found")
}
```

The type checker will not let you call methods on an `Option[User]` as if it were a `User`. You must unwrap it, and the act of unwrapping forces you to decide what to do in the absent case. The tradeoff is that code that was previously two lines (assign, then use) may become four lines (assign, match, handle both cases). Bramble mitigates this with combinators like `map`, `unwrap_or`, and `?` propagation for `Option` in pipelines, so the explicit handling rarely becomes noise.

## Ergonomics matter

Replacing null with `Option` only improves reliability if programmers do not fight the type system to avoid it. Bramble therefore makes `Option` easy to work with. The `?` operator in a function that returns `Option[T]` unwraps a `Some` and short-circuits to `None` on absence, which covers the common "pass the absence up the call stack" pattern without ceremony.

```bramble
fn display_name(id: UserId) -> Option[String] {
    let user = find_user(id)?   // returns None early if not found
    Some(user.name)
}
```

> **Note:** `Option` and `Result` interact. A function that can both fail and produce no value typically returns `Result[Option[T], Error]`, which looks nested at first glance but cleanly separates "the operation failed" from "the operation succeeded but found nothing."

## Migration cost and prior art

If you are coming from a language where null is pervasive, the adjustment is real. The instinct to return null from a function that has nothing useful to say must be replaced with a habit of reaching for `Option`. Code reviews in early Bramble teams frequently surfaced attempts to encode absence as a sentinel value (empty string, `-1`, a boolean flag alongside the value) rather than using `Option` properly. That pattern defeats the purpose.

Once the habit is established, the benefit is significant: the question "can this be null?" disappears from code review. The type tells you. See [working with optionals](xref:bramble.how-to.language.work-with-optionals) for the practical patterns, or [errors as values](xref:bramble.explanation.errors-as-values) for how `Result` extends the same idea to failure cases.
