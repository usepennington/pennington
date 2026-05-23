---
title: "Work with optionals"
description: "Use Option, Some, and None to represent values that may be absent without reaching for null."
uid: bramble.how-to.language.work-with-optionals
order: 120
sectionLabel: "Language"
tags: [option, null-safety, pattern-matching, types]
---

Bramble has no `null`. Values that may be absent are represented as `Option<T>`, which is either `Some(value)` or `None`. This makes absence visible in the type system and forces you to handle it.

## Produce an Option value

Return `Some(x)` when a value exists and `None` when it does not. Standard library functions follow the same convention.

```bramble
fn find_user(id: int) -> Option<User> {
    let users = db::query("SELECT * FROM users WHERE id = ?", [id])
    if users.len() == 0 {
        return None
    }
    Some(users[0])
}
```

## Unwrap with a fallback using unwrap_or

`unwrap_or` extracts the inner value or returns a default when the `Option` is `None`.

```bramble
let name = find_user(42)
    .map(|u| u.display_name)
    .unwrap_or("anonymous")
```

The closure passed to `map` is only called when the value is `Some`. `None` passes through unchanged.

## Pattern match for full control

When you need different behaviour for each arm, `match` is explicit and exhaustive.

```bramble
match find_user(req.user_id) {
    Some(user) => render_profile(user),
    None       => render_404("user not found"),
}
```

## Propagate None with ?

The `?` operator works on `Option` as well as `Result`. Applied to `None` inside a function that returns `Option`, it returns `None` immediately.

```bramble
fn greeting(id: int) -> Option<str> {
    let user = find_user(id)?
    let prefs = load_prefs(user.prefs_id)?
    Some("Hello, ${user.name}! Language: ${prefs.lang}")
}
```

Each `?` exits early with `None` if the preceding expression is `None`, so the happy path reads without nested matches.

## Chain transformations with and_then

`and_then` (sometimes called flat-map) lets you chain functions that themselves return `Option`, avoiding nested `Some(Some(...))`.

```bramble
let avatar_url = find_user(id)
    .and_then(|u| load_prefs(u.prefs_id))
    .and_then(|p| p.avatar_url)
    .unwrap_or("/img/default-avatar.png")
```

> [!NOTE]
> `map` wraps the return value in `Some` automatically. Use `and_then` when your closure itself returns an `Option` to keep the chain flat.

For how `Option` interacts with `Result` in fallible pipelines, see [handle errors with Result](xref:bramble.how-to.language.handle-errors-with-result). The underlying type design is covered in [the type system](xref:bramble.explanation.the-type-system).
