---
title: "Tab platform or language variants together"
description: "Wrap whole sections of prose, code, and lists in DocFX-style content tabs — synced page-wide, with dependent tabs for a second axis."
uid: how-to.rich-content.content-tabs
order: 5
sectionLabel: "Rich Content"
tags: [authoring, markdown, tabs, content-tabs]
---

When one instruction has per-platform or per-language variants — install steps for macOS, Linux, and Windows; the same API in C# and F# — content tabs let the reader pick one and read a self-contained walk-through. Unlike a [tabbed code block](xref:how-to.code-samples.tabbed-code), a content tab holds a whole section: paragraphs, lists, callouts, and code together.

## Before you begin
- An existing Pennington site renders Markdown (see <xref:tutorials.getting-started.first-site> if not).
- The pipeline was built through `AddPennington` / `AddDocSite` / `AddBlogSite`, so `UseContentTabs()` is already wired into the default `MarkdownPipelineFactory`.
- The default [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) integration, or a stylesheet, targets the `ctabs` / `ctab-btn` / `ctab-panel` classes.

## Author a tab group

Each tab opens with a level-1 heading whose link points at `#tab/<id>`. The link text becomes the button label; the `<id>` identifies the tab. A horizontal rule (`---`) ends the group. Everything between two tab headings is that tab's panel.

````markdown
# [macOS](#tab/macos)

Install the SDK with the official `.pkg` installer or Homebrew's cask.

- Confirm the install with `dotnet --list-sdks`.
- On Apple Silicon, grab the `arm64` build.

# [Linux](#tab/linux)

Install `dotnet-sdk-11.0` from your distribution's package feed.

```bash
sudo apt-get install dotnet-sdk-11.0
```

# [Windows](#tab/windows)

Install the SDK with the Windows installer or `winget`.

```powershell
winget install Microsoft.DotNet.SDK.11
```

---
````

# [macOS](#tab/macos)

Install the SDK with the official `.pkg` installer or Homebrew's cask.

- Confirm the install with `dotnet --list-sdks`.
- On Apple Silicon, grab the `arm64` build.

# [Linux](#tab/linux)

Install `dotnet-sdk-11.0` from your distribution's package feed.

```bash
sudo apt-get install dotnet-sdk-11.0
```

# [Windows](#tab/windows)

Install the SDK with the Windows installer or `winget`.

```powershell
winget install Microsoft.DotNet.SDK.11
```

---

The first tab is active by default. Each panel is ordinary Markdown — the bullet list and fenced code above both render with the page's normal prose styling.

## Tabs sync across the page

Tab `<id>`s are page-wide. Every group that shares an id selects together: pick **Linux** once and every `#tab/linux` on the page follows. The client unions id sets across all co-occurring groups, and the choice persists in `localStorage` per set, so the selection carries across pages with the same tab groupings as well as within the current page. Switch a tab in the group above and watch this one match:

# [macOS](#tab/macos)

You picked macOS — paths use `/Users/you`.

# [Linux](#tab/linux)

You picked Linux — paths use `/home/you`.

# [Windows](#tab/windows)

You picked Windows — paths use `C:\Users\you`.

---

## Dependent tabs

A third path segment — `#tab/<id>/<condition>` — makes a tab depend on another group's selection. The `<condition>` is itself a tab id selected elsewhere. Use it when one choice (the OS) should drive a second (the tool), without making the reader pick twice.

````markdown
# [.NET CLI](#tab/tool/linux)

Run `dotnet run` from a bash shell.

# [.NET CLI](#tab/tool/windows)

Run `dotnet run` from PowerShell.

# [Editor](#tab/editor/linux)

Open the folder with `code .`.

# [Editor](#tab/editor/windows)

Open the folder with `code .` or Visual Studio.

---
````

The group below shows **.NET CLI** and **Editor** as its two buttons; each has a Linux and a Windows variant. It follows the platform you picked above — switch **macOS / Linux / Windows** there and the panel here re-resolves.

# [.NET CLI](#tab/tool/linux)

Run `dotnet run` from a bash shell — the dev host binds `http://localhost:5000`.

# [.NET CLI](#tab/tool/windows)

Run `dotnet run` from PowerShell — the dev host binds `http://localhost:5000`.

# [Editor](#tab/editor/linux)

Open the project with `code .` and use the C# Dev Kit extension.

# [Editor](#tab/editor/windows)

Open the project with `code .`, or load the `.slnx` in Visual Studio.

---

A condition that names an id with no selector of its own never resolves, so give every condition a plain tab group somewhere on the page.

## What the renderer emits

A group renders as a `ctabs` container holding a `ctabs-bar` tab strip and one `ctab-panel` per tab. The tab strip carries `not-prose` so the buttons stay out of page typography; the panels deliberately do **not**, so panel content renders with full prose styling.

See <xref:reference.markdown.extensions> for the full element, attribute, and class reference.

## Verify

- The group renders as a tab strip with one button per heading, not as stacked headings — the first panel shows and the rest are hidden.
- Clicking a button switches the visible panel; two groups that share ids switch together when you pick one.
- View source — the container is a `<div class="ctabs" data-content-tabs>` holding a `ctabs-bar` strip and one `ctab-panel` per tab, with `data-active` on the first.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — content tabs alongside every other non-CommonMark feature
- How-to: <xref:how-to.code-samples.tabbed-code> — tab code-only variants inside a single code block
- How-to: <xref:how-to.pages.include-shared-content> — pull a shared partial into a tab panel
- How-to: <xref:how-to.rich-content.alerts> — callouts, which sit naturally inside a tab panel
