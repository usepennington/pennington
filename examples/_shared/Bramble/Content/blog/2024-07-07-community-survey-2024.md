---
title: "The 2024 community survey"
description: "Indira Cole shares results from the first Bramble community survey, covering who's using the language, what they're building, and what they want next."
date: 2024-07-07
author: Indira Cole
tags: [community, survey, roadmap, ecosystem]
uid: bramble.blog.community-survey-2024
---

We ran the first Bramble community survey in June, and 847 people filled it out. That's more than I expected for a language that didn't exist eighteen months ago. This post is the honest summary: the good numbers, the sobering numbers, and what we're going to do about them.

## Who responded

The respondents skew technical and relatively experienced. About 71% reported five or more years of professional programming experience. The most common prior languages were Python (68%), Rust (41%), and Go (38%). That matches what I see in the forums — people who found scripting languages too loose and systems languages too demanding.

About 34% said they use Bramble at work. Of that group, the most common deployment contexts were internal tooling, CI/CD scripting, and embedded scripting inside larger applications — exactly the niches we designed for.

## What people are building

We asked respondents to describe their primary Bramble project in a free-text field. I read every one. The categories that emerged:

| Category | Share |
|---|---|
| Internal automation / tooling | 31% |
| Application scripting (embedded) | 24% |
| CLI tools published via Thicket | 19% |
| Data processing pipelines | 14% |
| Hobby / learning | 12% |

The "embedded scripting" number is higher than I expected at this stage. Several teams at companies like Tinroof and Quill & Co. are using Bramble as a configuration and extension language inside Bramble-agnostic host applications. The sandbox model is doing its job there.

## What people want

The top five requested improvements, ranked by mention count:

1. **Better async ergonomics** — 61% of respondents mentioned this. The 1.2 async/tasks release is already in progress; this result pushed it up the priority list.
2. **More standard library coverage** — particularly around text processing, HTTP clients, and date/time handling.
3. **IDE support** — language server, completions, go-to-definition. We have a prototype LSP implementation that isn't public yet.
4. **Improved compile times on large projects** — expected; incremental compilation is on the 2.0 roadmap.
5. **More Thicket packages** — this one is mostly on the community, and it's moving in the right direction.

## The numbers I'll be honest about

52% of respondents said Bramble's documentation is "good" or "excellent". That sounds fine until you notice that 18% said it was "poor" or "confusing". The tutorials score highest; the reference material scores lowest. We're putting dedicated effort into the reference section this quarter.

Error message quality got a 4.1/5.0 average, which I'll take. The borrow checker errors specifically got called out positively by 23 people, which will please Hazel.

Only 29% of respondents said they'd recommended Bramble to a colleague. That's the number I'm most focused on changing. The barrier is almost always "I can't point them at a definitive resource". Better docs, a better landing page, and the upcoming IDE support should move that needle.

## What comes next

The survey results are informing the 1.2 and 2.0 roadmaps directly. Async/tasks in 1.2 is confirmed. Standard library expansion is queued for 1.3. IDE support is targeting early 2025.

If you want to see the [full explanation of the concurrency model](xref:bramble.explanation.concurrency-model) we're building toward in 1.2, that document reflects the current design. Things may change as we finish implementation, but the intent is stable.

We'll run the survey again in January. Thank you to everyone who filled it out, and especially to the 312 people who left detailed written feedback. We read all of it.
