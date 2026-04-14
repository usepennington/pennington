# Post-mortem — BeyondRoslynExample

## Shape

Two-csproj app: host (`BeyondRoslynExample.csproj`, DocSite + Roslyn) at
the folder root, sibling library (`Sample/BeyondRoslynExample.Sample.csproj`)
holding xmldocid targets. Inner `BeyondRoslynExample.slnx` at the folder
root registers **only the Sample library** — the host csproj does not
need to be in the inner slnx (Roslyn loads the workspace purely to
resolve xmldocid strings, and the host's `Program.cs` top-level
statements are not targets). Both csprojs are in main `Pennington.slnx`
under `/examples/`.

One gotcha: the host csproj root-globs `.cs` files, which by default
scoops up `Sample/*.cs` too and duplicate-definitions the Sample
assembly attributes at host build. Fix is a single `DefaultItemExcludes`
property (`$(DefaultItemExcludes);Sample\**`) — no `<Compile Remove>`
needed.

## API reality

- Extension: `Pennington.Roslyn.RoslynExtensions.AddPenningtonRoslyn(
  this IServiceCollection, Action<RoslynOptions>? configure = null)`.
- Options type: `Pennington.Roslyn.RoslynOptions` with
  `SolutionPath` (string?) and `ProjectFilter` (nullable record).
- `SolutionPath` is resolved **relative to the host's working directory
  at runtime** (the csproj folder under `dotnet run`). `"BeyondRoslynExample.slnx"`
  works without any `./` or `../` prefix.

## Fence syntax (verified in source)

`RoslynCodeBlockPreprocessor.ParseLanguageId` substring-matches the
fence info string for `:xmldocid-diff`, `:xmldocid`, or `:path`. The
**body** of the fence carries XmlDocIds (one per line, two for `-diff`),
**not** a `key="value"` attribute on the fence. So:

````markdown
```csharp:xmldocid
T:Ns.Type
```
````

Append `,bodyonly` → `csharp:xmldocid,bodyonly` to strip declarations
and render just the block/expression body. Multiple XmlDocIds in one
fence are concatenated with a blank line between each fragment.

## `:path` quirk (found during verification)

`ProcessPath` computes `Path.GetDirectoryName(SolutionPath)`; when
`SolutionPath` is a bare filename (`"BeyondRoslynExample.slnx"`) this
returns empty string and the preprocessor emits "Solution directory not
found". Docs site dodges this by using `"../../Pennington.slnx"`. For
this example I dropped the `:path` fence — the tutorial's teaching
surface is `xmldocid`, not `path`. Future app should either prefix
`SolutionPath` with `./` (doesn't help — still empty dir) or a real
`../` parent.

## Verification

Full-solution `dotnet build Pennington.slnx` clean. Dev server on
`localhost:5631`: Playwright confirmed `/api-pulls` renders five fenced
code blocks, zero error placeholders — `T:Calculator` shows the full
class with XML docs, `M:Add` shows the method with signature, bodyonly
on `Multiply` shows just `return a * b;`, `T:Greeter` renders the class,
and the multi-line fence produced two concatenated fragments. Static
`dotnet run -- build output` produced 8 pages; grep confirmed the
generated `api-pulls/index.html` contains `return a * b;`, `Adds two
integers`, and `Builds friendly greetings` — no "Error:" or "Symbol not
found" strings. Output cleaned.

No blockers.
