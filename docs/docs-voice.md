# Doc Voice Guide

This document defines the writing voice for all documentation. It blends two influences:

- **Sarah Maddox**: Reader-first empathy. Always write from the reader's position, not the product's architecture. Anticipate what they don't know yet and meet them there.
- **Adam Wathan**: Confident, efficient prose. Short declarative sentences. Explain "why" once, then show the thing. Casual connective tissue without devolving into banter.

The result: a voice that respects the reader's intelligence *and* their context. A knowledgeable colleague who talks with you, not at you, and never wastes your time.

---

## Core Voice Principles

### Be confident, not arrogant
State things directly. Don't hedge with "you might want to consider perhaps using" — just say "use." But don't be dismissive. The reader is smart; they just don't know *your* thing yet.

**Yes:** "Pass the template path as the first argument."
**No:** "You might want to consider passing the template path as the first argument."
**Also no:** "Obviously, you pass the template path as the first argument."

### Be warm, not chatty
Casual tone is fine. Banter is not. One informal aside per page is plenty. If a sentence exists only to be friendly and teaches nothing, cut it.

**Yes:** "This is where things get interesting."
**No:** "Okay, buckle up buttercup, because this next part is wild! 🚀"

### Explain why, then show how
One or two sentences of context before the code. Not zero (the reader needs orientation). Not five (they came here to do something).

### Assume competence, not knowledge
The reader knows C# and .NET. They don't know your API surface. Never explain what a `class` is. Always explain what *your* class does and why they'd reach for it.

### Write from the reader's chair
Before writing a sentence, ask: "Does the reader need this right now?" If the answer is "only if they've read a different page first," either add the context or link to it. Every page should make sense to someone who landed here from a search engine.

### Use present tense
"The method returns a `RenderResult`" — not "will return." The code does what it does right now.

### Put conditions before instructions
"To customize the output directory, set the `OutputPath` property" — not "Set the `OutputPath` property to customize the output directory." The reader scans for their situation first, then reads the action.

### Never use
- "Simply", "just", "easy", "obviously" — they make struggling readers feel stupid.
- "Please" — it implies the step is optional.
- "As we discussed earlier" — every page stands alone.
- Latin abbreviations in prose (write "for example" not "e.g.", write "that is" not "i.e.").

---

## Diataxis Mode-Specific Voice

The documentation uses the Diataxis framework. Each mode has a distinct purpose and the voice shifts to match. The core personality stays the same — the *register* changes.

### Tutorials (learning-oriented)

**Goal:** Take beginners from zero to a working result.

**Voice register:** Encouraging, patient, like a friendly teacher. This is the warmest the voice gets. You're walking alongside the reader. Celebrate small wins briefly ("That's it — you've got a working site"), then move on.

**Structural rules:**
- Step-by-step, building complexity gradually.
- Every step must produce a visible result. If it doesn't, the step is too abstract — restructure.
- Show the exact code they should have at each checkpoint. Don't make them guess.
- Don't explain *why* something works unless it's needed to complete the next step. Save the theory for Explanation pages and link to it.

**Language:** "Let's...", "You'll...", "Now we can...", "At this point you should see..."

**Example:**
> Let's create your first page. Add a new file called `index.md` to the `content` directory:
>
> ```csharp
> // code here
> ```
>
> Run the build again. You should see your page rendered at `output/index.html`.

### How-to Guides (problem-oriented)

**Goal:** Solve specific, real-world problems.

**Voice register:** Direct, efficient, solutions-focused. The reader has a problem right now. Respect their urgency. This is the most Wathan-like mode — get in, solve it, get out.

**Structural rules:**
- Title should describe the outcome, not the feature. "Deploy to GitHub Pages" not "Using the GitHub Pages Integration."
- Open with one sentence of context if the reader needs it. Often they don't.
- Clear numbered steps to achieve the goal. No detours.
- Don't teach concepts here. If they need background, link to the Explanation page.

**Language:** "To do X...", "When you need...", "This approach...", "If you're working with..."

**Example:**
> ## Add a custom 404 page
>
> Create a file called `404.md` in your content root. The generator picks it up automatically — no configuration needed.
>
> If you need custom markup instead of markdown, use a `.html` file instead. The generator treats it as a passthrough.

### Explanations (understanding-oriented)

**Goal:** Clarify and illuminate concepts.

**Voice register:** Thoughtful, informative, like a knowledgeable colleague talking through a design decision. This is where "why" lives. You can be more discursive here than anywhere else, but still respect the reader's time — no meandering.

**Structural rules:**
- Topic-based, connecting ideas. Not step-by-step.
- It's fine to compare approaches, discuss tradeoffs, and share the reasoning behind design decisions.
- Code examples here are illustrative, not instructional. They show *how something works*, not *what to type*.
- Link to the How-to Guide when the reader is ready to act on what they've learned.

**Language:** "The reason...", "This happens because...", "Consider...", "The tradeoff here is..."

**Example:**
> ## How the rendering pipeline works
>
> Content files pass through three stages: parsing, transformation, and output. Each stage is a discrete step you can hook into, which means you can modify content at exactly the point that makes sense for your use case.
>
> The pipeline processes files independently by default. This is a deliberate choice — it keeps builds fast and makes caching straightforward. The tradeoff is that cross-page features like tag indexes require an explicit collection step.

### Reference (information-oriented)

**Goal:** Provide accurate, comprehensive information.

**Voice register:** Neutral, precise, authoritative. This is the driest the voice gets. The reader is looking something up mid-task. Personality takes a back seat to scannability and accuracy.

**Structural rules:**
- Systematic, complete coverage. Every parameter, every option, every return type.
- Use consistent formatting — if one method entry has a parameters table, they all do.
- Keep descriptions short. One sentence for what it does. A second sentence only if the behavior has a non-obvious gotcha.
- Don't explain when to use something. That's what How-to and Explanation pages are for. Link to them.

**Language:** "This parameter...", "Returns...", "Available options...", "Defaults to..."

**Example:**
> ### `OutputPath`
>
> **Type:** `string`
> **Default:** `"./output"`
>
> The directory where generated files are written. Relative paths resolve from the project root. The directory is created if it doesn't exist.

---

## Formatting Conventions

- **Headings:** Sentence case. "Configure the build pipeline" not "Configure the Build Pipeline." We do NOT need a title in the docs, the engine adds H1
- **Code references in prose:** Use backticks for anything the reader would type or see in code — type names, method names, parameter names, file paths, CLI commands.
- **Code blocks:** Always include the language identifier. Use comments sparingly — only to highlight what changed or what matters. Don't comment obvious lines.
- **Lists:** Use them for genuinely parallel items (parameters, options, requirements). Don't use a bullet list where a sentence would do.
- **Admonitions/callouts:** Use for genuine warnings and non-obvious gotchas. Not for emphasis, not for tips that are really just the next sentence of the explanation. One per page is usually enough. Two is the ceiling.
- **Links:** Link on first mention of a concept that has its own page. Use descriptive link text ("see the pipeline explanation" not "click here").