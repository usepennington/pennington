---
title: "Teaching Bramble to beginners"
description: "Reflections from a semester of teaching Bramble in an introductory programming course: what clicks immediately, what causes confusion, and what the language does unexpectedly well."
date: 2025-10-02
author: Juniper Sato
tags: [community, education, learning, language]
uid: bramble.blog.teaching-bramble
---

This past spring I ran Bramble as the primary language in an introductory programming course at a small technical college. Thirty-two students, most of whom had never written code before. I want to share what I observed — the good, the genuinely surprising, and the rough edges — because I think it's useful signal for how the language evolves.

## What clicks right away

The REPL ("the patch") is excellent for teaching. Students can type an expression, see a value, form a hypothesis, test it. The feedback loop is tight and forgiving. Hedge made this even better for the first two weeks before anyone had installed anything locally — I could share a link with a skeleton program and everyone had the same environment immediately.

Pattern matching was the thing I expected to struggle to teach that turned out to be almost universally intuitive. When I showed students `match` as "ask what shape this value has, then handle each shape," they understood it faster than they understood `if/else` chains. I think the exhaustiveness check helped: the compiler telling you "you haven't handled the `None` case" is more pedagogically useful than "your program crashed at runtime."

```bramble
fn describe(score: Option<Int>) -> Str {
    match score {
        Option.Some(n) if n >= 90 => "excellent",
        Option.Some(n) if n >= 70 => "passing",
        Option.Some(_)            => "needs work",
        Option.None               => "not submitted",
    }
}
```

Every student in the class could write something like this by week four. I haven't been able to say that about equivalent constructs in other languages I've taught.

## What trips people up

Ownership. Not because it's conceptually hard, but because Bramble's ownership is *almost* invisible — the compiler handles most of it implicitly — until it isn't, and then the error message describes something the student hasn't thought about yet.

The most common confusion was around passing a value to a function and then trying to use it again. Bramble's borrowing model allows this with references, but the syntax (`&` prefix, `borrow` keyword in function signatures) looks unfamiliar. Students frequently asked "why can't I use it, I didn't delete it." The honest answer is that explaining ownership well takes a full lecture, and once I dedicated time to it properly, most students got it. The lectures I scheduled for week two probably should have been week four, after students had seen the pattern enough times to have intuitions to attach the explanation to.

The other friction point was `Result`. Students understood that functions could fail. What they found confusing was *why* they had to handle the `Result` at the call site rather than letting it propagate automatically. Introducing `?` earlier helped, but then I had students using `?` everywhere including in contexts where it didn't type-check, which generated confusing errors.

## What the language does unexpectedly well

No `null` made a measurable difference. In courses I've taught with languages that have null, students reliably hit null-reference errors in their first substantial program. None of my Bramble students hit an equivalent surprise. They hit type errors, ownership errors, and exhaustiveness errors — all at compile time, all with a specific message. That's a different category of learning experience.

The error message quality is good enough that several students said they would "just read what the compiler said" before asking for help. For a beginner, that's a significant win.

The [installation tutorial](xref:bramble.tutorials.install-bramble) was the on-ramp I assigned for homework before the first session. It's well-paced and the students who did it arrived with a working environment and a reasonable mental model of the toolchain. The students who skipped it did not have a good first class.

## Would I teach it again?

Yes, without hesitation. The language's constraints turn out to be teaching aids. When the compiler rejects something, it's almost always explaining a real concept rather than surfacing an arbitrary rule. That's a rare quality.
