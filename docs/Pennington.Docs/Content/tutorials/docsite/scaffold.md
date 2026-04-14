---
title: "Scaffold a documentation site with DocSite"
description: "Replace a barebones Pennington host with AddDocSite + UseDocSite + RunDocSiteAsync, configure DocSiteOptions, and understand how Areas map to top-level content folders."
section: "docsite"
order: 10
tags: []
uid: tutorials.docsite.scaffold
isDraft: true
search: false
llms: false
---

> **In this page.** Replacing the barebones setup with `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`, configuring `DocSiteOptions` (site title, GitHub URL, header/footer), and understanding how areas map to top-level folders.
>
> **Not in this page.** Authoring markdown content (covered next) or overriding the DocSite layout — treated as a customization how-to.

## What you'll do

- **Artifact:** a running DocSite with a branded header, footer, GitHub link, and one rendered documentation page.
- **Skill:** you'll know how to swap a basic Pennington host for the DocSite template and fill in the options that make the shell feel real.

## Prerequisites

- .NET 11 SDK installed
- Completed [Create your first Pennington site](/tutorials/getting-started/first-site)
- A shell and text editor or IDE

The finished code for this tutorial lives in [`examples/TempoDocsExample`](https://github.com/usepennington/pennington/tree/main/examples/TempoDocsExample) and, for the richer header/footer variation, [`examples/BeaconDocsExample`](https://github.com/usepennington/pennington/tree/main/examples/BeaconDocsExample).

---

## 1. Swap the core host for the DocSite template

_Replace the basic host with the DocSite setup so you get the docs shell in one move._

### Step 1.1 — Replace Program.cs with the minimal DocSite startup

- Open `Program.cs` from the site you created in the earlier tutorial.
- Replace the basic `AddPennington` setup with `AddDocSite`, `UseDocSite`, and `RunDocSiteAsync`.
- Set `SiteTitle` and `Description` so the site has a clear identity from the first run.

```csharp file="examples/TempoDocsExample/Program.cs"
```

_This is the smallest working DocSite startup you can copy from._

### Step 1.2 — Create a single placeholder page

- Add `Content/index.md` if it does not already exist.
- Give it a `title:` and one short paragraph of body text.
- Keep it simple. This tutorial is about getting the shell working.

### Checkpoint — the DocSite shell renders

- Run `dotnet run`
- Visit `http://localhost:5000/`
- Confirm the header, sidebar, and article area all render

---

## 2. Add the header badge, footer, and GitHub link

_Now make the default shell feel like your site rather than a placeholder._

### Step 2.1 — Copy the Beacon header/footer snippet into your options record

- Add `HeaderIcon`, `HeaderContent`, `FooterContent`, and `GitHubUrl` to your options block.
- Keep your own site title and description.
- Use the example as a starting point, then change the text to match your project.

```csharp file="examples/BeaconDocsExample/Program.cs"
```

_This example shows a fuller DocSite shell with icon, badge, footer, and GitHub link._

### Step 2.2 — Verify the rewritten header renders

- Refresh the site.
- Confirm the icon, badge, and footer content all render.
- Click the GitHub link to make sure it goes where you expect.

### Checkpoint — the branded shell is complete

- The header shows your title and extra branding
- The footer shows your custom content
- The GitHub link works

---

## 3. Add one more touch of site identity

_Finish by setting the visual direction so the starter shell feels intentional._

### Step 3.1 — Set the color scheme

- Add a `ColorScheme` to your `DocSiteOptions`.
- Pick one direction and keep it simple for now.
- Refresh the site and confirm the colors changed across the shell.

```csharp file="examples/TempoDocsExample/Program.cs"
```

_This example shows a complete options block with a color scheme in place._

### Step 3.2 — Reload the finished shell

- Visit the home page one more time.
- Confirm the header, sidebar, article body, footer, and colors all feel like one site.
- Leave advanced structure such as multiple areas for a later how-to.

### Checkpoint — the site feels real

- The shell has your branding, colors, and links
- The content page still renders inside the DocSite layout
- You have a solid base for the next tutorial on writing docs pages

---

## Summary

- You replaced the basic Pennington host with the DocSite template
- You configured the key shell options that make the site feel branded
- You rendered a real markdown page inside the DocSite layout
- You now have a clean starting point for writing documentation pages

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
