---
title: "How the type system works"
description: "A conceptual overview of Bramble's static type system, including inference, records, generics, traits, and the features it deliberately leaves out."
uid: bramble.explanation.the-type-system
order: 30
sectionLabel: "Explanation"
tags: [types, inference, generics, traits, records]
---

Bramble's type system sits in a deliberate middle ground: expressive enough to model most real-world domains without ceremony, constrained enough that the type checker's errors are actionable rather than cryptic. Understanding what it includes—and what it leaves out—explains many of the language's ergonomic choices.

## Static typing with global inference

All values in Bramble have a type known at compile time. The programmer does not have to write most of those types explicitly because the type checker infers them from context. Inference in Bramble uses a Hindley-Milner-style algorithm with extensions for records and trait bounds, which means inference is generally complete: if a program type-checks, the inferred types are the most general correct types for those expressions.

In practice, annotations are expected at function signatures and type definitions, and optional everywhere else. This is a deliberate policy, not a limitation. Function signatures serve as documentation that the type checker verifies; omitting them forces future readers to reconstruct intent from inference traces.

```bramble
fn clamp(value: Float, lo: Float, hi: Float) -> Float {
    let bounded = if value < lo { lo } else { value }  // type inferred
    if bounded > hi { hi } else { bounded }
}
```

## Records and structural typing

Bramble uses records as its primary product type. Records are structurally typed for read-only contexts: any value that has at least the required fields satisfies a record type used as a function parameter. This allows lightweight composition without declaring an explicit hierarchy.

Mutating a borrowed record requires the type to match exactly, not structurally, because the borrow checker needs to track field-level aliasing precisely. This distinction is one of the more surprising edges for newcomers, but it prevents a class of subtle aliasing errors.

## Generics and traits

Generics are parametric: a function or type can be parameterized over a type variable constrained by one or more traits.

```bramble
fn largest[T: Ord](items: List[T]) -> Option[T] {
    items.reduce(|a, b| if a > b { a } else { b })
}
```

Traits describe capabilities—`Ord` for ordered comparison, `Display` for string formatting, `Clone` for explicit copying—and implementations are declared separately from type definitions. The trait system is intentionally less powerful than some alternatives: Bramble does not support higher-kinded types or associated types with complex variance rules. The tradeoff is that the type system remains predictable. A beginner encountering a trait error gets a message they can act on.

## What the type system omits

Bramble has no subtype polymorphism in the object-oriented sense. There is no inheritance, no class hierarchy, no casting. Polymorphism is achieved through traits (for behavior) and sum types (for alternatives). The reason is that subtype hierarchies tend to encode domain knowledge in the wrong place—in the type relationships—rather than in the logic of the program.

Dependent types, refinement types, and effect types are out of scope for 1.x. They are powerful tools, but each significantly increases the complexity of type error messages and the mental model required to write code. Bramble's position is that a type system is only as useful as the errors it produces are readable.

For the full catalog of built-in types and their trait implementations, see the [types reference](xref:bramble.reference.language.types). The relationship between the type system and the borrow checker is covered in [ownership and borrowing](xref:bramble.explanation.ownership-and-borrowing).
