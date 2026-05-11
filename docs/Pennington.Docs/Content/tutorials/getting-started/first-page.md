---
title: "Add your first markdown page"
description: "Write front-matter-driven markdown files and watch Pennington turn them into URLs and navigation on its own."
sectionLabel: "Getting Started with Pennington"
order: 101020
tags:
  - front-matter
  - markdown
  - navigation
  - routing
uid: tutorials.getting-started.first-page
---

By the end of this tutorial a site runs at `http://localhost:5000` with three markdown pages (`/`, `/about`, `/contact`) and a nav strip that sorts itself — with no edits to `Program.cs` after step 1.

The tutorial covers how Pennington maps a `Content/**/*.md` path directly to a URL, what the `title:` key does for the page title and nav label, and how `order:` sorts siblings without any routing code.

## Prerequisites

- .NET 11 SDK installed
- Completed [Spin up a minimal Pennington site](xref:tutorials.getting-started.first-site) (or that example's Program.cs ready to reuse)
- A code editor that renders YAML front matter cleanly (VS Code, Rider, etc.)

The finished code for this tutorial lives in [`examples/GettingStartedFirstPageExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedFirstPageExample).

---

## 1. Write a single page with required front matter

Starting from the minimal site built in the previous tutorial, this step adds a real front-matter block and turns a single markdown file into a routed, titled page.

<Steps>
<Step StepNumber="1">

**Drop `Content/index.md` into the project**

Create a `Content/` folder at the project root if it isn't there yet — the previous tutorial already pointed `ContentRootPath` there. Add a file named `index.md` with a YAML front-matter block between two `---` fences. Pennington's `FrontMatterParser` reads that block into a `DocFrontMatter` record; `title` is the only key required to render a page. Any markdown body works below the closing fence.

```markdown:path
examples/GettingStartedFirstPageExample/Content/index.md
```

The `title:` value flows to both the HTML `<title>` tag and the nav link label. For the full range of front-matter capability interfaces, see <xref:explanation.core.front-matter-capabilities> — for now, `title` is enough.

</Step>
<Step StepNumber="2">

**Confirm the host from the previous tutorial is unchanged**

`Program.cs` calls `AddPennington`, registers `AddMarkdownContent<DocFrontMatter>`, applies `UsePennington`, and maps every route with a single `MapGet("/{*path}", ...)` that walks `IContentService` instances. The only addition since the previous tutorial is a `NavigationBuilder` injection — nothing else changes for the rest of this tutorial.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedFirstPageExample.Stage1.Run(System.String[])
```

Notice the `NavigationBuilder.BuildTree(tocItems)` call and the string join that becomes `navHtml` — that's the piece that grows in later steps without any edits. The flat join here only renders the top level; once a section gains nested children, switch to <xref:reference.ui.navigation>'s `TableOfContentsNavigation` component, which walks the full `Children` tree.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` from the example project
- Visit `http://localhost:5000/`
- The page shows the heading **Welcome to the site** and a nav strip with one link: **Welcome** pointing at `/`

</Checkpoint>

---

## 2. Let the file path become the URL

Now let's add a second file and watch Pennington map the on-disk path straight to a route — no router-table edits required.

<Steps>
<Step StepNumber="1">

**Add `Content/about.md` with its own front matter**

Create `about.md` in the same `Content/` folder. The filename (minus `.md`) becomes the URL segment: `about.md` serves at `/about`. Set `order: 20` so this file sorts predictably when the third one arrives. A short body — a paragraph or two — is enough.

```markdown:path
examples/GettingStartedFirstPageExample/Content/about.md
```

Keep an eye on the `order: 20` line — its role becomes apparent once the third file lands in step 3.

</Step>
<Step StepNumber="2">

**Reload and confirm the host code is still the same**

The Stage 2 host method delegates entirely to `Stage1.Run` — zero code changes between steps 1 and 2. The only thing that moved was a file on disk.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedFirstPageExample.Stage2.Run(System.String[])
```

`Stage2.Run(args) => Stage1.Run(args)` is intentional — the point is that the host is untouched.

</Step>
</Steps>

<Checkpoint>

- With the host still running (or after a `dotnet run` restart), visit `http://localhost:5000/about`
- The page shows the heading **About this site** and a nav strip with two links: **Welcome** (`/`) and **About** (`/about`)
- Revisit `/` — the same two-item nav strip appears there too

</Checkpoint>

---

## 3. Watch navigation auto-assemble from a third file

With two pages confirmed, let's add a third and see both URL mapping and front-matter ordering click into place together.

<Steps>
<Step StepNumber="1">

**Add `Content/contact.md` with `order: 30`**

The `order:` field is how Pennington sorts siblings in the nav tree. Setting `order: 30` here — higher than About's `order: 20` — places Contact after About. The root `index.md` carries no `order:` and sorts first by convention.

```markdown:path
examples/GettingStartedFirstPageExample/Content/contact.md
```

The example body invites a filename rename — that's coming in step 3.3.

</Step>
<Step StepNumber="2">

**Confirm the host is still unchanged in Stage 3**

Stage 3 also delegates to `Stage1.Run`. Three files on disk, one host method, nothing edited between any of the stages.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedFirstPageExample.Stage3.Run(System.String[])
```

The `NavigationBuilder` injected back in step 1.2 is what produces the three-item nav — it's been at work the whole time.

</Step>
<Step StepNumber="3">

**Rename `contact.md` to see the URL follow the file**

With the host running, rename `Content/contact.md` to `Content/reach-out.md`. On the next request the nav link's href becomes `/reach-out` — no config, no restart. This is file-path-to-URL mapping in action. Rename it back to `contact.md` before continuing so later tutorials match.

</Step>
</Steps>

<Checkpoint>

- Visit `/`, `/about`, and `/contact` in turn — each renders its own H1 and body
- The nav strip on every page lists three links in this order: **Welcome**, **About**, **Contact**
- Temporarily rename `contact.md` to `reach-out.md` and refresh — the nav link's href becomes `/reach-out`; rename it back afterward

</Checkpoint>

---

## Summary

- A Pennington-ready markdown page needs a YAML front-matter block and the required `title:` key.
- Any `Content/**/*.md` path becomes a URL automatically — no route table, no registration per file.
- The nav strip builds itself from the content folder, sorted by the `order:` field, without changes to `Program.cs`.
- Adding or renaming a markdown file predictably updates the URL and nav position.

