---
title: "Publish your docs as Markdown for AI agents"
description: "Discover the Markdown copy DocSite already publishes for every page, see how agents find it, brand the llms.txt front door, keep a page out of the agent corpus, and give a Razor landing page a machine-readable twin."
sectionLabel: "Beyond the Basics"
order: 3
tags:
  - llms-txt
  - markdown
  - agents
  - docsite
uid: tutorials.beyond-basics.markdown-for-agents
---

When a developer points an AI coding agent at your library, the agent usually goes to your docs for the answer. Hand it an HTML page and most of what it reads is chrome — navigation, scripts, styling, SVG icons — with the prose it actually wants buried inside. Hand it clean Markdown and every token goes to content, so the agent answers more accurately, and the code it writes against your library is more likely to be right.

DocSite already publishes that Markdown for you. `AddDocSite` wires up `AddLlmsTxt`, so the moment your site runs, every page has a Markdown copy and `/llms.txt` lists them all — there's nothing to switch on. (It's a core feature, not a DocSite one: on a bare `AddPennington` host it's a single `AddLlmsTxt` call — see [the llms.txt how-to](xref:how-to.feeds.llms-txt).) This tutorial makes that work visible. You'll fetch the copies an agent would fetch, confirm an agent can find them, then shape them to fit your site: branding the `/llms.txt` front door, holding one page back from agents, and giving a Razor landing page a Markdown twin.

## Prerequisites

- .NET 10 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (provides the running DocSite host this tutorial builds on)
- A terminal with `curl` (every checkpoint below fetches a URL so you can see the actual bytes an agent receives)

You'll work in the DocSite project from the scaffold tutorial. Keep `dotnet run` going in one terminal and run the `curl` commands in another.

---

## 1. See your docs the way an agent does

Your running site serves HTML at every page URL. Alongside each one, it also publishes a Markdown copy at the same URL with `.md` appended — and an index of everything at `/llms.txt`. Let's add a page to look at, then fetch both forms.

<Steps>
<Step StepNumber="1">

**Add a page to inspect**

Create `Content/guides/install.md` so there's a concrete page to fetch. The front matter is the ordinary DocSite shape.

````markdown
---
title: "Install"
description: "Add the package and wire the host."
---

## Install the package

```
dotnet add package Pennington
```

Then call `AddPennington` in your host and point it at a `Content/` folder.
````

</Step>
<Step StepNumber="2">

**Fetch the page as HTML, then as Markdown**

The page renders at `/guides/install/`. Its Markdown copy lives at `/guides/install.md`.

```bash
curl http://localhost:5000/guides/install/
curl http://localhost:5000/guides/install.md
```

The first command returns a full HTML document — `<head>`, scripts, navigation chrome, the works. The second returns just the page, as Markdown, with a small YAML header and the body underneath:

````markdown
---
title: Install
description: Add the package and wire the host.
canonical_url: http://localhost:5000/guides/install/
content_hash: sha256:…
tokens: 41
---

## Install the package

```
dotnet add package Pennington
```

Then call `AddPennington` in your host and point it at a `Content/` folder.
````

</Step>
</Steps>

<Checkpoint>

- `curl http://localhost:5000/guides/install.md` returns Markdown with a `Content-Type: text/markdown` response — not HTML
- The header carries `canonical_url`, `content_hash`, and a `tokens` estimate, so a budget-aware agent knows what it's fetching before it commits
- `curl http://localhost:5000/llms.txt` returns an index that lists *Install* under its section, linked to `/guides/install.md`

</Checkpoint>

---

## 2. See how an agent finds the Markdown

Generating the Markdown isn't enough — an agent has to discover it. DocSite advertises every page's copy two ways, and you can see both from the page you already have.

The first is a `<link rel="alternate">` tag in the page's `<head>`. View the source of `/guides/install/` and look near the other metadata:

```html
<link rel="alternate" type="text/markdown" href="/guides/install.md">
```

This is the standard way to declare an alternate representation of a page. Claude Code's WebFetch sends an `Accept: text/markdown` header and looks for exactly this signal; other agents read it the same way. The second route is `/llms.txt` itself — a crawlable map of the whole corpus, with per-section grouping and token estimates so an agent can plan which pages to pull.

<Checkpoint>

- The HTML at `/guides/install/` contains a `<link rel="alternate" type="text/markdown" …>` pointing at `/guides/install.md`
- `/llms.txt` lists the same `.md` URL, so an agent that starts from the index reaches the page without ever parsing HTML

</Checkpoint>

---

## 3. Brand the front door

The top of `/llms.txt` is the first thing an agent reads. By default it uses your site title and description. To give it a fuller introduction — what the project is, what an agent should know before diving in — drop a header file in your content root.

<Steps>
<Step StepNumber="1">

**Add `Content/llms-header.txt`**

The file is named `llms-header.txt`, not `llms.txt`, on purpose: a file literally named `llms.txt` in your content root would be served verbatim and shadow the generated index. Everything in this file becomes the preamble above the generated page list.

```text
# Acme Widgets Docs

> The official documentation for Acme Widgets, a .NET content toolkit.

These docs cover installation, configuration, and the public API. Code samples
are pulled from the live source, so they compile. Prefer the Markdown copies
linked below over scraping the HTML pages.
```

</Step>
</Steps>

<Checkpoint>

- `curl http://localhost:5000/llms.txt` now opens with your header text, followed by the generated index
- The section list and per-page links below the header are unchanged — you've replaced only the preamble

</Checkpoint>

---

## 4. Keep a page out of the agent corpus

Some pages aren't worth an agent's tokens — a thank-you page, a redirect stub, a placeholder. Set `llms: false` in a page's front matter and DocSite leaves it out of everything machine-facing while still serving it to humans.

<Steps>
<Step StepNumber="1">

**Add `llms: false` to the install page's front matter**

You'll undo this in a moment — it's here so you can watch the page leave the agent corpus.

```markdown
---
title: "Install"
description: "Add the package and wire the host."
llms: false
---
```

</Step>
</Steps>

<Checkpoint>

- `curl http://localhost:5000/guides/install/` still returns the HTML page — humans are unaffected
- `curl http://localhost:5000/guides/install.md` now returns a 404 — there is no Markdown copy
- The page no longer appears in `/llms.txt`, and its `<link rel="alternate">` tag is gone from the HTML `<head>`
- Remove the `llms: false` line again before moving on, so the page rejoins the corpus

</Checkpoint>

---

## 5. Give a Razor landing page a Markdown twin

A page backed by Markdown gets its `.md` copy for free, as you saw in section 1. A page backed by a Razor component — a marketing landing page, say — does not: there's no source Markdown to publish, and converting a splash full of layout would hand an agent the same noise you're trying to avoid. If you've built a [Razor landing page](xref:tutorials.docsite.landing-page), write its Markdown twin as an agent-only page.

<Steps>
<Step StepNumber="1">

**Add `Content/index.llms.md`**

A file whose name ends in `.llms.md` is agent-only: it joins `/llms.txt` and gets a Markdown copy, but it never renders as an HTML page — so it won't collide with the Razor landing at `/`. Its copy lands at the route's URL with `.md` appended, which for the root is `/index.md`. Write the orientation you'd want an agent to read.

```markdown
---
title: "Acme Widgets — docs home"
description: "Machine-readable orientation for agents."
---

Acme Widgets is a .NET content toolkit. The page at `/` is a marketing landing
page; this is its machine-readable equivalent.

- `/llms.txt` — the full index of every page as Markdown.
- Start with `/guides/install.md`, then read the rest from the index.
```

</Step>
<Step StepNumber="2">

**Point the landing page at it**

A `.llms.md` page produces no HTML, so it can't advertise itself. Add the alternate link to your landing component's `<HeadContent>`, so an agent on the home page is steered to the Markdown twin the same way it would be on any other page.

```razor
<HeadContent>
    <link rel="alternate" type="text/markdown" href="/index.md" />
</HeadContent>
```

</Step>
</Steps>

<Checkpoint>

- `curl http://localhost:5000/` returns your marketing HTML, unchanged
- `curl http://localhost:5000/index.md` returns your orientation as `text/markdown`
- The source of `/` carries `<link rel="alternate" type="text/markdown" href="/index.md">`, and `/llms.txt` lists the home entry

</Checkpoint>

---

## Summary

- DocSite publishes a Markdown copy of every page at its URL with `.md` appended, plus an `/llms.txt` index — both wired automatically by `AddDocSite`.
- Each page advertises its Markdown copy with a `<link rel="alternate" type="text/markdown">` tag and through `/llms.txt`, the two routes agents use to find it.
- A `Content/llms-header.txt` file replaces the front-door preamble without touching the generated page list.
- `llms: false` in a page's front matter keeps it human-visible but removes it from the Markdown copies, the index, and the alternate-link advertisement.
- A Razor-backed page has no Markdown copy of its own; write its twin as an agent-only `*.llms.md` page (it joins `/llms.txt` and gets a `.md` copy but renders no HTML) and advertise it with an alternate link.
