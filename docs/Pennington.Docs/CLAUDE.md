Documentation site, built using Pennington.

Uses the Diataxis framework. All articles should be in one of these quadrants and adhere to the guidelines

### Tutorials (learning-oriented)
- **Goal**: Take beginners from zero to a working result
- **Tone**: Encouraging, patient, like a friendly teacher
- **Structure**: Step-by-step, building complexity gradually
- **Focus**: What the user will accomplish and learn
- **Language**: Variety of, but not limited to "Let's...", "You'll...", "Now we can..."

### How-to Guides (problem-oriented)
- **Goal**: Solve specific, real-world problems
- **Tone**: Direct, efficient, solutions-focused
- **Structure**: Clear steps to achieve the goal
- **Focus**: Practical solutions to common needs
- **Language**: Variety of, but not limited to "To do X...", "When you need...", "This approach..."

### Explanations (understanding-oriented)
- **Goal**: Clarify and illuminate concepts
- **Tone**: Thoughtful, informative, like a knowledgeable colleague
- **Structure**: Topic-based, connecting ideas
- **Focus**: Why things work the way they do
- **Language**:: Variety of, but not limited to "The reason...", "This happens because...", "Consider..."

### Reference (information-oriented)
- **Goal**: Provide accurate, comprehensive information
- **Tone**: Neutral, precise, authoritative
- **Structure**: Systematic, complete coverage
- **Focus**: Facts, parameters, specifications
- **Language**: Variety of, but not limited to"This parameter...", "Returns...", "Available options..."

## Code-block embedding syntax

Pennington preprocesses fenced code blocks whose info string ends in `:path`, `:xmldocid`, or `:xmldocid-diff`. The language before the colon (`csharp`, `razor`, `text`, etc.) drives highlighting. Do not use `raw-file="…"` — that form is not parsed.

### Embed a whole file — `<lang>:path`
Body is one file path, relative to the solution directory (where `Pennington.slnx` lives).

````markdown
```csharp:path
examples/AlexBlogExample/Program.cs
```
````

### Embed a symbol — `<lang>:xmldocid`
Body is one XmlDocId per line (e.g. `T:Ns.Type`, `M:Ns.Type.Method`, `P:Ns.Type.Prop`). Multiple IDs are concatenated in the output.

````markdown
```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.AddBlogSite(...)
```
````

Add `,bodyonly` to strip the declaration and render only the method/property body:

````markdown
```csharp:xmldocid,bodyonly
M:Ns.Type.Method
```
````

### Diff two symbols — `<lang>:xmldocid-diff`
Body must contain exactly 2 XmlDocIds (before/after). Supports the same `,bodyonly` suffix.

### When to use which
- **`:xmldocid`** — default for C# examples; survives renames, refactors, and line shifts.
- **`:xmldocid,bodyonly`** — when the declaration is noise (showing what's inside a method).
- **`:path`** — only when no xmldocid-addressable symbol exists: top-level-statements `Program.cs`, Razor components, markdown/content files, config files.
- **`:xmldocid-diff`** — before/after comparisons in explanation pages.

Requires `Pennington.Roslyn` wired (`AddPenningtonRoslyn`) and `SolutionPath` set on `RoslynOptions` / `DocSiteOptions`.