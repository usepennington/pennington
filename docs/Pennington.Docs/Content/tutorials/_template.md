---
title: "TEMPLATE — Tutorial"
description: "Template for tutorial pages. Duplicate, rename, and replace every placeholder before publishing."
section: "Template"
order: 9999
tags: []
uid: template.tutorial
isDraft: true
search: false
llms: false
---

> **In this page.** _One sentence — paste from `docs-toc.md` "Covers" line, trim if needed._
>
> **Not in this page.** _One sentence pointing to the neighboring how-to / reference / explanation — paste from `docs-toc.md` "Does not cover" line._

## What you'll do

_**Artifact** (one sentence): the concrete thing the reader will have running when they finish — e.g., "a Pennington site with two locales and a working language switcher"._

_**Skill** (one sentence): what the reader will know how to do afterward — e.g., "you'll understand how to wire locale-prefixed URLs to translated content"._

## Prerequisites

_Keep this list to **tools installed** and **prior tutorials completed** — nothing else. If a reader needs to read an Explanation page before step 1, this tutorial is scoped wrong; pull the needed concept into a short sentence at the start of the step that uses it._

- .NET 11 SDK installed
- Completed [_Prior tutorial title_](/_path_) (or have an equivalent Pennington project ready)
- _Additional tool prerequisite if any_

The finished code for this tutorial lives in [`examples/_ExampleProjectName_`](https://github.com/usepennington/pennington/tree/main/examples/_ExampleProjectName_).

---

## 1. _First unit title — verb-led, e.g., "Register the service"_

_One sentence: what this unit accomplishes and why it has to come first._

### Step 1.1 — _Action verb + target_

_One sentence of setup if the step needs it._

> _xmldocid tip: prefer short, lesson-scoped methods over whole `Program.cs` files. Use `bodyonly` when the declaration line is noise. If the example project has no symbol small enough, that's a signal to add one — not to inline code here._

```csharp:xmldocid,bodyonly
M:_ExampleProjectName_._Type_._Member_
```

_Explain one non-obvious line if the code has any. Delete this paragraph if nothing needs it._

### Step 1.2 — _Next action_

_Continue until the unit is done. Keep each step to one verb + one target._

```csharp:xmldocid
T:_ExampleProjectName_._Type_
```

### Checkpoint — _What you should see now_

_Concrete verifiable state: a URL returns something, a file exists, a log line appears. If the reader cannot verify, the unit is wrong — tighten it._

- Run `dotnet run` and visit `http://localhost:5000/_path_`
- You should see _X_

---

## 2. _Second unit title_

_Repeat the unit/step/checkpoint structure. Aim for 3–5 units per tutorial. More than 5 means this should probably be two tutorials._

### Step 2.1 — _Action_

```csharp:xmldocid,bodyonly
M:_ExampleProjectName_._Type_._Member_
```

### Checkpoint — _What you should see now_

_Concrete verification._

---

## Summary

_Three to five bullets. Each bullet names a capability the reader now has, not a topic you covered._

- You wired up _X_ and saw it _Y_.
- You learned to _Z_.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
