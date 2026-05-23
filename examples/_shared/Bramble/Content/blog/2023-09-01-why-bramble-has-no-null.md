---
title: "Why Bramble has no null"
description: "Maple Okafor makes the design case for Option over null, arguing that optional absence is a type-level fact, not a runtime surprise."
date: 2023-09-01
author: Maple Okafor
tags: [design, null-safety, option, language]
uid: bramble.blog.why-bramble-has-no-null
---

I want to make the design case for one of Bramble's most discussed decisions: the language has no `null`. Not a restricted null, not a nullable annotation layer on top of a nullable runtime — no null at all. This post is about why that choice is correct, and why it's not as radical as it sounds.

## The problem isn't null values

Here's what I'd argue: the problem was never that null values exist. Absence is a real concept. A user might not have a display name. A cache lookup might come up empty. These are legitimate states.

The problem is that traditional null is *untyped absence*. When a function returns `String`, does it mean "always a string" or "a string, or null if I felt like it"? You don't know from the type. You know from the documentation, if it exists, if it's accurate, if you read it carefully enough that day.

Bramble's answer is `Option<T>`. A function returning `Option<String>` means exactly one thing: you'll get either `Some(value)` or `None`, and the type says so. A function returning `String` means you'll always get a string.

```bramble
fn find_display_name(user: User) -> Option<String> {
    if user.display_name.len() > 0 {
        Some(user.display_name)
    } else {
        None
    }
}
```

The caller cannot ignore the `Option`. If they try to pass it somewhere expecting `String`, the compiler rejects it. The absence is visible — always, by construction.

## "But Option is just nullable with extra steps"

This objection comes up constantly. I understand it: if `Option<String>` can be `None` and `String?` can be null, aren't they the same thing?

No, for one critical reason: in a language with `String?`, `String` still might be null if something lied or if there's a hole in the null-analysis rules. Null escapes the type system in ways that `Option` cannot. `Option<String>` in Bramble is a sealed union type. There is no way to produce a `String` that is secretly absent.

The other difference is forcing explicitness at the point of handling. You can't call methods on `Option<String>` as if it were a `String`. You unwrap first, via pattern matching or the `?` operator, and the unwrapping is visible in the code.

## The propagation operator

Explicit handling doesn't have to be verbose. The `?` operator propagates `None` outward from the current function, as long as the function returns `Option`:

```bramble
fn greeting(id: UserId) -> Option<String> {
    let user = find_user(id)?           // returns None if not found
    let name = find_display_name(user)? // returns None if no display name
    Some("Hello, {name}!")
}
```

This is about as concise as nullable chaining in other languages, but the return type of `greeting` makes the absence explicit to every caller.

## Was it worth it?

Every codebase has a place where something was nil that shouldn't have been. Usually it's two or three layers away from where the problem was introduced. I've spent more hours in those codebases than I want to count.

The design question is whether you want those bugs to surface at compile time or at runtime. Bramble votes compile time. The [detailed explanation of the Option design](xref:bramble.explanation.why-no-null) goes deeper into the type theory and the tradeoffs we considered and rejected.

Not every language needs to make this choice. But for a language designed fresh, making the type system ignorant of absence feels like leaving money on the table.
