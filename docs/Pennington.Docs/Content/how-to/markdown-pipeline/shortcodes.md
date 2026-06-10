---
title: "Expand a directive before Markdig parses"
description: "Register an IShortcode handler so directives in markdown expand to text or HTML before the rest of the pipeline runs."
uid: how-to.markdown-pipeline.shortcodes
order: 4
sectionLabel: "Markdown Pipeline"
tags: [extensibility, markdown, shortcodes]
---

To stamp a value into a page — a version string, a repo link, a build timestamp — implement `IShortcode`. The handler runs **before** Markdig parses the page, so the string it returns becomes part of the markdown source and flows through the rest of the pipeline as if you had typed it yourself.

> [!NOTE]
> The shortcode expander only runs on markdown pages. Razor templates, HTML files, and other content services bypass it.

## Write a shortcode

Implement `IShortcode` as a sealed class. `ExecuteAsync` receives the parsed invocation — positional args, named args, and inline content (null for self-closing tags) — plus the host page's route and metadata. Return the string that should replace the directive in the markdown source. Return raw HTML when the output should not be re-parsed as markdown.

The lab below claims `GitHubRepo` and turns a positional repo slug into an anchor:

```csharp:symbol
examples/ExtensibilityLabExample/GitHubRepoShortcode.cs > GitHubRepoShortcode
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

A markdown page that mixes the custom handler with a built-in:

```markdown:symbol
examples/ExtensibilityLabExample/Content/shortcodes-demo.md
```

After expansion, the prose reads as if the version and link were authored inline. The fenced code block at the bottom is left alone so the demo page can document the syntax it uses.

## Verify

On your own site, add the handler and call it from a page:

- Register the handler after `AddPennington`, drop `\<?# GitHubRepo "owner/repo" /?>` into any markdown page, and load that page in a browser — the directive renders as a working link to the GitHub repo. View source and look for `data-extensibility-lab="github-repo-shortcode"` on the anchor (rename the attribute to your own once you copy the handler). That attribute means `ExecuteAsync` produced the HTML rather than Markdig parsing it as markdown.
- Static build: run your build (`dotnet run -- build output`) and grep the emitted HTML for the link to confirm the expander runs during publish, not only under `dotnet run`.
- If the directive renders verbatim instead of expanding, the handler's `Name` does not match the directive (names are case-insensitive but must otherwise match) or the registration ran before `AddPennington`.

The lab ships a complete worked version:

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/shortcodes-demo/` — the page shows the host's version and a working link to the GitHub repo.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep the emitted HTML for the version string to confirm the expander runs during publish.

## Syntax and built-ins

For the full directive grammar, the shipped `Version` and `PackageVersion` shortcodes, and the error-degradation contract, see the [Shortcodes section of the Markdown extensions catalog](xref:reference.markdown.extensions#shortcodes). The essentials follow.

A shortcode call has three shapes:

```markdown
\<?# Name /?>                               ← self-closing
\<?# Name positional key=value /?>          ← positional and named arguments
\<?# Name ?>inline content\<?#/ Name ?>     ← block form with content
```

Names are case-insensitive and match the handler's `Name` property. Values that contain whitespace must be double-quoted: `title="A Long Title"`. Shortcodes expand everywhere in the markdown source — including inside fenced code blocks — so install snippets can carry the real version string straight out of the build:

```bash
dotnet add package Pennington --version <?# PackageVersion /?>
```

To *document* the syntax rather than call into it, prefix the opener with a backslash. The expander consumes the backslash and emits the directive as-is; Markdig HTML-encodes the angle brackets downstream so the reader sees the literal text in their rendered prose or code sample.

```markdown
Run \\<?# Version /?> to stamp the host's version into a page.
```

Handler exceptions and unknown names degrade automatically: each produces an HTML comment in place of the directive (`<!-- Pennington: shortcode 'Name' failed: <message> -->`) plus a warning diagnostic, so one bad call site never fails the render. If a particular failure should fail the build, register a response processor that promotes the matching diagnostic to error severity.

## Related

- Reference: <xref:reference.api.i-shortcode> — the `IShortcode` interface and the `ShortcodeInvocation`/`ShortcodeContext` it receives.
- Reference: [Shortcodes in the Markdown extensions catalog](xref:reference.markdown.extensions#shortcodes) — directive grammar, built-ins, and error semantics.
- How-to: [Add a custom fence syntax](xref:how-to.markdown-pipeline.code-block-preprocessor) — intercept fenced code blocks instead of inline directives.
