---
title: "Expand a directive before Markdig parses"
description: "Register an IShortcode handler so directives in markdown expand to text or HTML before the rest of the pipeline runs."
uid: how-to.markdown-pipeline.shortcodes
order: 209030
sectionLabel: "Markdown Pipeline"
tags: [extensibility, markdown, shortcodes]
---

To stamp a value into a page — a version string, a repo link, a build timestamp — implement `IShortcode`. The handler runs **before** Markdig parses the page, so the string it returns becomes part of the markdown source and flows through the rest of the pipeline as if you had typed it yourself.

> [!NOTE]
> The shortcode expander only runs on markdown pages. Razor templates, HTML files, and other content services bypass it.

## The built-in: Version

Pennington ships one shortcode — `AssemblyVersionShortcode`, dispatched by the name `Version`. It reads the entry assembly and emits the host application's version string.

```markdown
Running on Pennington \<?# Version /?>.

Major branch: \<?# Version format=major /?>.
```

The `format` named argument accepts `full` (default), `major`, `minor`, and `informational`. The last reads `AssemblyInformationalVersionAttribute`, which captures the Git SHA and pre-release suffixes set by MSBuild.

## Syntax

A shortcode call has three shapes:

```markdown
\<?# Name /?>                               ← self-closing
\<?# Name positional key=value /?>          ← positional and named arguments
\<?# Name ?>inline content\<?#/ Name ?>     ← block form with content
```

Names are case-insensitive and match the handler's `Name` property. Values that contain whitespace must be double-quoted: `title="A Long Title"`.

## Expand inside fenced code blocks

Shortcodes expand everywhere in the markdown source — including inside fenced code blocks — so install snippets can carry the real version string straight out of the build:

```bash
dotnet add package Pennington --version <?# Version /?>
```

## Errors degrade automatically

Handler exceptions are caught by the expander. Each failure produces two artifacts:

- An HTML comment in place of the directive — `<!-- Pennington: shortcode 'Name' failed: <message> -->` — so the page still renders and the surrounding prose still flows.
- A warning diagnostic carrying the exception message. In build mode it appears in the final "WARNINGS" list printed by `dotnet run -- build`; in dev mode it shows up on the diagnostic overlay and the `X-Pennington-Diagnostic` response header.

Unknown shortcode names follow the same pattern (`<!-- Pennington: unknown shortcode 'Name' -->` + warning). No special handling is required in the handler — write guard clauses with `throw`, and let the framework do the bookkeeping. If a particular failure should actually fail the build, register a response processor that promotes the matching diagnostic to error severity.

## Escape a literal directive

When the goal is to *document* the syntax rather than call into it, prefix the opener with a backslash. The expander consumes the backslash and emits the directive as-is; Markdig HTML-encodes the angle brackets downstream so the reader sees the literal text in their rendered prose or code sample.

```markdown
Run \\<?# Version /?> to stamp the host's version into a page.
```

## Write a shortcode

Implement `IShortcode` as a sealed class. `ExecuteAsync` receives the parsed invocation — positional args, named args, and inline content (null for self-closing tags) — plus the host page's route and metadata. Return the string that should replace the directive in the markdown source. Return raw HTML when the output should not be re-parsed as markdown.

The lab below claims `GitHubRepo` and turns a positional repo slug into an anchor:

```csharp:xmldocid
T:ExtensibilityLabExample.GitHubRepoShortcode
```

A few patterns worth copying:

- **HTML-encode untrusted input.** `WebUtility.HtmlEncode` everywhere a user-supplied value touches the output. The string is spliced into the source and re-rendered, so injection vectors are real.
- **Throw idiomatic guard clauses.** The expander catches every handler exception, surfaces the message as a build warning, and emits an HTML comment in place. One bad call site never fails the build.
- **Lean on context.** `context.Route.SourceFile` tells the handler which page invoked it — useful for path-relative resolution.

## Register the handler

Pennington collects every `IShortcode` from DI. Register anywhere after `AddPennington` — there is no `PenningtonOptions` knob.

```csharp
builder.Services.AddSingleton<IShortcode, GitHubRepoShortcode>();
```

The expander reads `IEnumerable<IShortcode>` at construction; when two handlers share a `Name`, the last-registered wins. The built-in `Version` shortcode is registered as a singleton inside `AddPennington`, so any handler you register afterwards joins the same dispatch table.

## Result

A markdown page that mixes both shortcodes:

```markdown:path
examples/ExtensibilityLabExample/Content/shortcodes-demo.md
```

After expansion, the prose reads as if the version and link were authored inline. The fenced code block at the bottom is left alone so the demo page can document the syntax it uses.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/shortcodes-demo/` — the page shows the host's version and a working link to the GitHub repo.
- View source and look for `data-extensibility-lab="github-repo-shortcode"` on the anchor. That attribute means `GitHubRepoShortcode.ExecuteAsync` produced the HTML rather than Markdig parsing it as markdown.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep the emitted HTML for the version string to confirm the expander runs during publish, not just at request time.

## Related

- How-to: [Add a custom fence syntax](xref:how-to.markdown-pipeline.code-block-preprocessor) — intercept fenced code blocks instead of inline directives
