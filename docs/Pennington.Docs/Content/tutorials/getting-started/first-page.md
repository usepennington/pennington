---
title: "Add your first markdown page"
description: "Write front-matter-driven markdown files and watch Pennington turn them into URLs and navigation on its own."
sectionLabel: "Getting Started with Pennington"
order: 20
tags:
  - front-matter
  - markdown
  - navigation
  - routing
uid: tutorials.getting-started.first-page
---

> **In this page.** _One sentence paraphrasing the Covers line: the reader will write a YAML front-matter block with the required `title` key, see how a file path under `Content/` becomes a URL, and watch the nav strip auto-assemble as a second and third markdown file land on disk._
>
> **Not in this page.** _One sentence paraphrasing the Does-not-cover line: point to the explanation on capability interfaces at `xref:explanation.core.front-matter-capabilities`, and to the how-to on non-markdown content sources at `xref:how-to.extensibility.custom-content-service` — those are out of scope here._

## What you'll do

_**Artifact** (one sentence): name the visible end state — a running site at `http://localhost:5000` with three markdown pages (`/`, `/about`, `/contact`) and a nav strip listing all three in front-matter `order`. Mention that the reader will not touch `Program.cs` after step 1._

_**Skill** (one sentence): what the reader walks away knowing — how Pennington maps a `Content/**/*.md` path to a URL, what the `title:` key does for the page title and nav label, and that `order:` sorts siblings without any routing code._

## Prerequisites

_Keep this list short. The Stage1 host code uses `AddPennington`, `UsePennington`, and a minimal `MapGet` — that's why the previous tutorial is required. Don't explain the bare host here; link back to it._

- .NET 11 SDK installed
- Completed [Spin up a minimal Pennington site](xref:tutorials.getting-started.first-site) (or have that example's Program.cs ready to reuse)
- A code editor that renders YAML front matter cleanly (VS Code, Rider, etc.)

The finished code for this tutorial lives in [`examples/GettingStartedFirstPageExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedFirstPageExample).

---

## 1. Write a single page with required front matter

_One sentence: the reader starts from the minimal-site host and replaces whatever placeholder `index.md` they had with a real YAML front-matter block, establishing the `title:` key as the one non-negotiable field._

### Step 1.1 — Drop `Content/index.md` into the project

_Walk the reader through creating the `Content/` folder (the minimal-site tutorial already pointed `ContentRootPath` there) and adding a single `index.md`. Call out that the YAML block between the two `---` fences is parsed by `FrontMatterParser` into a `DocFrontMatter` record, and that `title` is the only key required to render a page. Encourage writing a short markdown body below the fences — anything goes._

```markdown:path
examples/GettingStartedFirstPageExample/Content/index.md
```

_Explain the two `---` fences and that `title:` flows through to both `<title>` and the nav label. Do not get into capability interfaces; link to the explanation page if curious readers need more._

### Step 1.2 — Confirm the host from the previous tutorial is unchanged

_Show the Stage 1 body so the reader can see the exact host code they should have: `AddPennington`, `AddMarkdownContent<DocFrontMatter>`, `UsePennington`, and the `MapGet("/{*path}", ...)` that walks `IContentService` instances. Emphasize that this is identical to the minimal-site end state plus a `NavigationBuilder` injection — nothing else will change for the rest of the tutorial._

```csharp:xmldocid,bodyonly
M:GettingStartedFirstPageExample.Stage1.Run(System.String[])
```

_Call out the `NavigationBuilder.BuildTree(tocItems)` call and the string join that becomes `navHtml` — that's the piece the reader will watch grow in later units without editing it._

### Checkpoint — A single page renders at `/`

_Concrete verification: the reader runs `dotnet run`, browses to the site root, sees the `Welcome` H1, and the nav strip shows exactly one link._

- Run `dotnet run` from the example project
- Visit `http://localhost:5000/`
- You should see the heading **Welcome to the site** and a nav strip with one link: **Welcome** pointing at `/`

---

## 2. Let the file path become the URL

_One sentence: the reader adds a second markdown file and discovers that Pennington maps the on-disk path straight to a route with no router-table edits._

### Step 2.1 — Add `Content/about.md` with its own front matter

_Have the reader create a sibling file in the same `Content/` folder. Emphasize that the filename (minus `.md`) becomes the URL segment: `about.md` will serve at `/about`. Use `order: 20` so this file sorts predictably when the third one lands. Keep the body short — a paragraph or two is plenty._

```markdown:path
examples/GettingStartedFirstPageExample/Content/about.md
```

_Note the `order: 20` line and promise that its role becomes obvious in unit 3. Do not explain `ISectionable` or capability interfaces here._

### Step 2.2 — Reload and confirm the host code is still the same

_Show the Stage 2 body to drive the point home: the method delegates to `Stage1.Run`, meaning zero code changes occurred between units 1 and 2. The only thing that moved was a file on disk. This is the lesson._

```csharp:xmldocid,bodyonly
M:GettingStartedFirstPageExample.Stage2.Run(System.String[])
```

_One sentence: call out that `Stage2.Run(args) => Stage1.Run(args)` is a deliberate choice — the tutorial's pedagogical point is that the host is untouched._

### Checkpoint — Two pages, two nav entries, zero code edits

_Concrete verification with both URLs and the expected nav list._

- With the host still running (or after a `dotnet run` restart), visit `http://localhost:5000/about`
- You should see the heading **About this site** and a nav strip with two links: **Welcome** (`/`) and **About** (`/about`)
- Revisit `/` — the same two-item nav strip appears there too

---

## 3. Watch navigation auto-assemble from a third file

_One sentence: the reader adds a third markdown file and sees both URL mapping and front-matter-driven ordering work together._

### Step 3.1 — Add `Content/contact.md` with `order: 30`

_Explain that `order:` in front matter is how Pennington sorts siblings in the nav tree. Setting `order: 30` here (vs `order: 20` on About) guarantees Contact lands after About. The root `index.md` has no `order:` and sorts first by convention — mention it briefly, do not dive into the sort algorithm._

```markdown:path
examples/GettingStartedFirstPageExample/Content/contact.md
```

_Point at the `order: 30` line and the one-paragraph body; note that the example body invites the reader to try a filename rename, which is step 3.3._

### Step 3.2 — Confirm the host is still unchanged in Stage 3

_Show the Stage 3 body — it also delegates to `Stage1.Run`. Three files on disk, one host method, nothing edited between stages. This seals the lesson._

```csharp:xmldocid,bodyonly
M:GettingStartedFirstPageExample.Stage3.Run(System.String[])
```

_Optional one-liner: the `NavigationBuilder` the reader saw injected back in step 1.2 is what produces the three-item nav now — they've been using it the whole time._

### Step 3.3 — Rename `contact.md` to see the URL follow the file

_Have the reader rename `Content/contact.md` to `Content/reach-out.md` with the host running. The nav entry updates to reflect the new URL on the next request. This is the concrete demonstration of file-path-to-URL mapping — no config, no restart needed if the dev host is watching._

_No code fence here; this is a filesystem action. Remind the reader to rename it back to `contact.md` before they move on so later tutorials match._

### Checkpoint — Three pages, sorted by front matter

_Concrete verification of the nav order and all three URLs._

- Visit `/`, `/about`, and `/contact` in turn — each should render its own H1 and body
- The nav strip on every page lists three links in this order: **Welcome**, **About**, **Contact**
- Temporarily rename `contact.md` to `reach-out.md` and refresh — the nav link's href becomes `/reach-out`; rename it back when you're done

---

## Summary

_Three to five bullets. Each one names a capability the reader now has. Do not recap the units — describe what the reader can now do on their own._

- You can write a Pennington-ready markdown page with a YAML front-matter block and the required `title:` key.
- You know that any `Content/**/*.md` path becomes a URL automatically — no route table, no registration per file.
- You've seen the nav strip build itself from the content folder, sorted by the `order:` field, without touching `Program.cs`.
- You can add and rename markdown files and predict the URL and nav position that result.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
