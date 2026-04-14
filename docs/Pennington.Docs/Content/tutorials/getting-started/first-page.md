---
title: "Add your first markdown page"
description: "Write a YAML front-matter block with the required title key, see the file path become a URL, and watch navigation auto-assemble as you add more files."
section: "getting-started"
order: 20
tags: []
uid: tutorials.getting-started.first-page
isDraft: true
search: false
llms: false
---

> **In this page.** Writing a YAML front-matter block, the required `title` key, how the file path becomes a URL, and seeing navigation auto-assemble as you add a second and third file.
>
> **Not in this page.** Custom front-matter types, capability interfaces, or non-markdown content sources.

## What you'll do

- **Artifact:** three markdown pages under `Content/`, each reachable at its own URL and visible in the running site.
- **Skill:** you'll know how to add a new page, give it the front matter it needs, and predict where it will show up.

## Prerequisites

- .NET 11 SDK installed
- Completed [Create your first Pennington site](/tutorials/getting-started/first-site/)
- A running site with `Content/index.md` already rendering

The finished state of this tutorial lives in [`examples/MinimalExample`](https://github.com/usepennington/pennington/tree/main/examples/MinimalExample).

---

## 1. Look at the page you already have

_Start by using the first page as your model._

### Step 1.1 — Open `Content/index.md`

- Open the file you created in the previous tutorial.
- Find the front-matter block at the top between the two `---` lines.
- Notice that the body of the page starts right after that block.

```markdown file="examples/MinimalExample/Content/index.md"
```

_Use this as the pattern for the new files you are about to add._

### Step 1.2 — Identify the one key you need

- Keep `title:` in every page you create.
- You can copy the rest of the shape from your existing page and change the values to match the new page.
- Leave custom front matter for a later guide.

### Checkpoint — you can name every part of the front-matter block

- You can point to the front matter and the markdown body
- You know each new page needs a `title:`
- The existing home page still renders

---

## 2. Add a second page in a sub-folder

_Now create a new page and see it show up in the running site._

### Step 2.1 — Create the sub-folder and the file

- Create `Content/sub-folder/` next to `index.md`.
- Add `page-one.md` with its own `title:` and a short body paragraph.
- Save the file.

```markdown file="examples/MinimalExample/Content/sub-folder/page-one.md"
```

_This example shows the shape of a second page in a sub-folder._

### Step 2.2 — Predict the URL before opening it

- Before you open the page, guess the URL: `/sub-folder/page-one`.
- The folder and file name become the path.
- You do not need to add a route anywhere else.

### Step 2.3 — Visit the URL and the home page

- Open `http://localhost:5000/sub-folder/page-one`.
- Confirm the page renders with the title you gave it.
- Go back to `http://localhost:5000/` and confirm the site now links to the new page.

### Checkpoint — two URLs, one new link

- `/sub-folder/page-one` loads successfully
- The home page now links to the new page
- You added one markdown file and the site picked it up

---

## 3. Add one more page

_Repeat the pattern once so it feels solid._

### Step 3.1 — Create `Content/sub-folder/page-two.md`

- Add `page-two.md` next to `page-one.md`.
- Give it its own `title:` and short body text.
- Open `/sub-folder/page-two` and confirm it loads.

```markdown file="examples/MinimalExample/Content/sub-folder/page-two.md"
```

_This example shows the third page in the finished tutorial state._

### Step 3.2 — Refresh the home page one more time

- Return to `http://localhost:5000/`.
- Confirm the home page now shows all three pages.
- Click through each one so you can see the pattern is repeatable.

### Checkpoint — three pages, three URLs, three list entries

- `/`, `/sub-folder/page-one`, and `/sub-folder/page-two` all render
- The home page lists all three pages
- You created the new pages without editing code or config

---

## Summary

- You can create a new markdown page with the front matter it needs
- You can predict the URL from the file path
- You can add more pages by repeating the same pattern
- You can see new pages appear in the running site without extra setup

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
