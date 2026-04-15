---
title: "TEMPLATE — How-to"
description: "Template for how-to pages. Duplicate, rename, and replace every placeholder before publishing."
sectionLabel: "Template"
order: 9999
tags: []
uid: template.howto
isDraft: true
search: false
llms: false
---

> **In this page.** _One sentence — paste from `docs-toc.md` "Covers" line, trim if needed._
>
> **Not in this page.** _One sentence pointing to the neighboring tutorial / reference / explanation — paste from `docs-toc.md` "Does not cover" line._

## When to use this

_One to two sentences. Name the goal the reader has arrived with ("You have a working Pennington site and want to host it under a sub-path"). Do not re-teach setup; link to the relevant tutorial if the reader landed here too early._

## Assumptions

_Bulleted list of what the page assumes is already true. Keep it short — long prerequisites suggest the page is really a tutorial._

- You have an existing Pennington site (see [_Getting Started tutorial_](/_path_) if not)
- _Other assumptions_

To copy a working setup, see [`examples/_ExampleProjectName_`](https://github.com/usepennington/pennington/tree/main/examples/_ExampleProjectName_). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Numbered, terse, verb-first. Aim for 3–7 steps. More than 7 means the page is really a tutorial._

<Steps>
<Step StepNumber="1">

**_Action verb + target_**

_One sentence of context if needed. Otherwise skip straight to the code._

```csharp:xmldocid,bodyonly
M:_ExampleProjectName_._Type_._Member_
```

</Step>
<Step StepNumber="2">

**_Next action_**

_One-sentence rationale only if non-obvious._

```csharp:xmldocid
T:_ExampleProjectName_._Type_
```

</Step>
<Step StepNumber="3">

**_Next action_**

_Add markdown / YAML / config snippets using plain fences when they are not C# symbols:_

```yaml
title: Sample Page
description: Short description
```

</Step>
</Steps>

---

## Verify

_Terse. One to three bullets. The reader should be able to confirm success without reading anything else._

- Run `dotnet run` and visit `/_path_`
- Expect _X_ to appear / expect log line `Y`
- _Additional check if warranted_

## Related

_Two to four cross-quadrant links. Point to the Reference pages for exhaustive lookup and the Explanation pages for background. Do not link to the next how-to in the same section — that is generated automatically._

- Reference: [_Options or API title_](/_path_)
- Background: [_Explanation title_](/_path_)
