Documentation site, built using Pennington.

Every article lives in exactly one Diataxis quadrant — tutorial, how-to, explanation, or reference — and obeys that quadrant's rules. The full voice guide is at `docs/docs-voice.md`; the essentials follow.

## Core voice

A confident colleague talking *with* the reader, not at them. Respect their intelligence and their time.

- **Confident, not arrogant.** State things directly: "Pass the template path." Not "you might want to consider…", not "obviously…".
- **Warm, not chatty.** One informal aside per page is plenty. Cut sentences that exist only to be friendly.
- **Why, then how.** One or two sentences of context, then the code. Not zero. Not five.
- **Assume competence, not knowledge.** The reader knows C# and .NET. They don't know *your* API. Never explain what a `class` is; always explain what *your* class does.
- **Reader's chair.** Every page stands alone. If context depends on another page, add it or link to it.
- **Present tense.** "The method returns a `RenderResult`" — not "will return."
- **Conditions before instructions.** "To customize the output directory, set `OutputPath`" — not "Set `OutputPath` to customize the output directory."

### Never use
- "Simply", "just", "easy", "obviously" — they make struggling readers feel stupid.
- "Please" — implies the step is optional.
- "As we discussed earlier" — every page stands alone.
- Latin abbreviations in prose ("for example" not "e.g."; "that is" not "i.e.").

## Diataxis modes

The personality stays the same; the *register* shifts.

### Tutorials (learning-oriented)
**Goal:** Zero to a working result. **Register:** Warmest — encouraging, patient, walking alongside the reader. Celebrate small wins briefly, then move on.
- Step-by-step; every step must produce a visible result. If it doesn't, the step is too abstract — restructure.
- Show the exact code they should have at each checkpoint. Don't make them guess.
- Don't explain *why* unless it's needed for the next step. Save theory for Explanation and link to it.
- **Language:** "Let's…", "You'll…", "Now we can…", "At this point you should see…"

### How-to guides (problem-oriented)
**Goal:** Solve a specific problem. **Register:** Direct, efficient — get in, solve it, get out.
- Title describes the outcome, not the feature: "Deploy to GitHub Pages", not "Using the GitHub Pages integration."
- Open with one sentence of context if needed; often it isn't.
- Use `<Steps>` only when each step depends on the previous one being done. For independent options or interface members, use H3 subheadings under a topical H2 — `<Steps>` implies ordering and readers will follow them in sequence. Pages that walk a feature member-by-member are reference, not how-to; re-frame around the user's goal or move them.
- Default page shape: enumerate variants under topical H2 + H3-per-variant. Inside each H3, write a fenced source block then the actual feature usage right below it so the rendered output appears next to its source — this is what the reader needs for alerts, code-block options, Mdazor components, front-matter keys, etc. Do not wrap variant pages in `<Steps>`.
- For non-visual output (CLI dump, sitemap.xml, llms.txt, build report), paste a real fenced block. `<RenderedFixture Path="examples/.../foo.md" />` is the exception — only when a complete composed configuration is the unit the reader needs to see; do not use it as a default "show output" mechanism.
- No concept teaching in the body; link to Explanation for background.
- **Language:** "To do X…", "When you need…", "If you're working with…"

### Explanations (understanding-oriented)
**Goal:** Illuminate concepts. **Register:** Thoughtful colleague talking through a design decision. Discursive is OK; meandering is not.
- Topic-based, connecting ideas. Not step-by-step.
- Compare approaches, discuss tradeoffs, share reasoning behind decisions.
- Code is illustrative, not instructional — shows *how something works*, not *what to type*.
- Link to the How-to when the reader is ready to act.
- **Language:** "The reason…", "This happens because…", "The tradeoff here is…"

### Reference (information-oriented)
**Goal:** Accurate, complete, scannable information. **Register:** Neutral, precise, authoritative — driest register; reader is looking something up mid-task.
- Systematic coverage — every parameter, option, return type.
- Consistent formatting across entries (if one has a parameters table, all do).
- One sentence per description; a second only for a non-obvious gotcha.
- Don't explain *when* to use something — link to How-to or Explanation.
- **Language:** "This parameter…", "Returns…", "Defaults to…"

## Formatting

- **Headings:** sentence case. "Configure the build pipeline", not "Configure the Build Pipeline." Do **not** add a top-level H1 — the engine adds it.
- **Code in prose:** backticks for anything the reader types or sees in code (types, methods, parameters, paths, CLI).
- **Code blocks:** always specify the language. Comment only what changed or what matters; don't comment obvious lines.
- **Lists:** only for genuinely parallel items. Don't bullet what a sentence would carry.
- **Admonitions/callouts:** for genuine warnings and non-obvious gotchas. One per page is usually enough; two is the ceiling.
- **Links:** on first mention of a concept with its own page. Descriptive link text ("see the pipeline explanation", not "click here").

## Code-block embedding syntax

This site embeds source through tree-sitter `:symbol` fences (not Roslyn). Pennington preprocesses fenced code blocks whose info string ends in `:symbol` or `:symbol-diff`. The language before the colon (`csharp`, `razor`, `text`, etc.) drives highlighting. Body paths are relative to the repo root (the tree-sitter `ContentRoot`). Do not use `raw-file="…"` — that form is not parsed.

### Embed a whole file — `<lang>:symbol`
Body is one file path with no member path. Works for any language (markdown, razor, json, css, …) — whole-file embedding needs no grammar.

````markdown
```csharp:symbol
examples/GettingStartedMinimalSiteExample/Program.cs
```
````

### Embed a member — `<lang>:symbol`
Body is `<file path> > <dotted member path>` — `Type`, `Type.Method`, `Type.Property`, or a nested `Type.Inner.Member`. Drop the namespace; the path is matched syntactically within the file. One reference per line; multiple lines are concatenated.

````markdown
```csharp:symbol
src/Pennington.BlogSite/BlogSiteServiceExtensions.cs > BlogSiteServiceExtensions.AddBlogSite
```
````

Add `,bodyonly` to strip the declaration and render only the method/property body:

````markdown
```csharp:symbol,bodyonly
src/Pennington/Pipeline/ContentPipeline.cs > ContentPipeline.ParseAsync
```
````

### Diff two members — `<lang>:symbol-diff`
Body must contain exactly 2 references (before/after), one per line. Supports the same `,bodyonly` suffix.

### When to use which
- **`:symbol`** — the default. A bare path embeds the whole file; `path > Type.Member` embeds one member. Language-agnostic.
- **`:symbol,bodyonly`** — when the declaration is noise (showing what's inside a method, or a type's members without its header).
- **`:symbol-diff`** — before/after comparisons in explanation pages.

### Caveats
- **Overloads** resolve to the first declaration of that name in the file — name-path addressing can't distinguish signatures. Point at an unambiguous member, or hand-write the snippet.
- **No `using` prepend.** Tree-sitter is syntactic; there is no `,usings` equivalent. Snippets render without import directives.

Requires `Pennington.TreeSitter` wired (`AddPenningtonTreeSitter`) with `ContentRoot` set.

> Some pages (`tutorials/beyond-basics/connect-roslyn.md`, `how-to/code-samples/focused-code-samples.md`) still document the Roslyn `:xmldocid`/`:path` fences as an optional library feature. That prose is historical — this site no longer executes those fences (`RoslynOptions.EnableCodeFragmentFences` is off).

## Writing conventions

### Internal links: uid/xref, never URL paths
Cross-links between docs pages use `[text](xref:uid)` or `<xref:uid>`. Never hardcode URL paths — they break silently when content moves or gets localized. The `uid` comes from the target page's front matter.

### Strip scaffolding callouts from final pages
Blockquotes of the form `> **In this page.** …` / `> **Not in this page.** …` are AI-directed scaffolding reminders, not reader-facing content. Delete them during final polish — they read as filler to a human.

### `<Steps>` numbering restarts per unit
Inside a `<Steps>` block, `StepNumber` counts from `1` within each unit. Do not use dotted `1.1` / `1.2` / `2.1` notation — there is no hierarchy inside a single steps block.