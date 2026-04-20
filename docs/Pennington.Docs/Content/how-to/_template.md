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

## When to use this

_One to two sentences. Name the goal the reader has arrived with ("You have a working Pennington site and want to host it under a sub-path"). The title must describe that outcome ("Deploy to GitHub Pages"), not the feature ("Using the GitHub Pages integration"). Do not re-teach setup; link to the relevant tutorial if the reader landed here too early._

## Assumptions

_Bulleted list of what the page assumes is already true. Keep it short — long prerequisites suggest the page is really a tutorial._

- You have an existing Pennington site (see [_Getting Started tutorial_](/_path_) if not)
- _Other assumptions_

To copy a working setup, see [`examples/_ExampleProjectName_`](https://github.com/usepennington/pennington/tree/main/examples/_ExampleProjectName_).

---

## Default page shape — variant pages

_Most how-to pages enumerate variants of one feature: alert kinds, code-block fence options, Mdazor components, front-matter keys. The right shape is one H2 per "topical bucket" plus one H3 per variant. Inside each H3, write a fenced source block and immediately below it the actual feature usage so the rendered output appears next to its source. Do **not** wrap the body in `<Steps>` — the variants are independent._

### _First variant_

_One sentence of context._

````markdown
> [!NOTE]
> The source goes in a fenced block so the reader sees the syntax.
````

> [!NOTE]
> Then the actual usage right below renders as the live output.

### _Second variant_

_One sentence of context._

````markdown
<Card Title="Hello">
This is the markdown source.
</Card>
````

<Card Title="Hello">
This is the markdown source.
</Card>

---

## When the work is sequential — use `<Steps>`

_Reach for `<Steps>` only when each step depends on the previous one being done — for example, a multi-stage build/deploy procedure. Keep it to 3–7 steps; more than that means the page is really a tutorial. The default page shape above is what most how-tos want; this is the exception._

<Steps>
<Step StepNumber="1">

**_Action verb + target_**

```csharp:xmldocid,bodyonly
M:_ExampleProjectName_._Type_._Member_
```

</Step>
<Step StepNumber="2">

**_Next action that depends on step 1_**

```csharp:xmldocid
T:_ExampleProjectName_._Type_
```

</Step>
</Steps>

---

## Showing non-visual output

_When the produced output is text (CLI dump, sitemap.xml, llms.txt, build report), paste a real fenced block:_

````markdown
```text
build complete: 42 pages, 0 warnings
```
````

_For a complete configuration that's better seen as a single rendered page (rare — only when the reader genuinely needs to see the whole composed result), embed the fixture inline:_

````markdown
<RenderedFixture Path="examples/_ExampleProjectName_/Content/_section_/_fixture_.md" Caption="..." />
````

_Default to inline source+output pairings instead. `<RenderedFixture>` is for the rare case where the whole composed page is the output, not the typical case where the page enumerates variants._

---

## Verify

_Terse. One to three bullets. The reader should be able to confirm success without reading anything else._

- Run `dotnet run` and visit `/_path_`
- Expect _X_ to appear / expect log line `Y`

## Related

_Two to four cross-quadrant links. Point to the Reference pages for exhaustive lookup and the Explanation pages for background. Do not link to the next how-to in the same section — that is generated automatically._

- Reference: [_Options or API title_](/_path_)
- Background: [_Explanation title_](/_path_)
