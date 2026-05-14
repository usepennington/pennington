# BeyondRoslynExample

Pulling live code into docs with `:xmldocid`, `:xmldocid,bodyonly`, `:xmldocid-diff`, and `:path` fence modifiers. `AddPenningtonRoslyn` points an MSBuild workspace at the sibling `Sample/` library; markdown fences resolve real symbols from there.

## Concepts

- `AddPenningtonRoslyn` (MSBuild workspace, symbol extraction, xmldoc → HTML)
- `RoslynOptions.SolutionPath` — relative to the host's working directory
- `<DefaultItemExcludes>` in the csproj keeping `Sample/` out of the host's compile

## Tutorial stages

`Stage1_NoRoslyn.cs` → `Stage2_AddRoslyn.cs`.

The inner `BeyondRoslynExample.slnx` plus the `Sample/` class library are part of the teaching surface — the tutorial pulls `T:BeyondRoslynExample.Sample.Calculator` and `M:...Greeter.Greet(System.String)` through `csharp:xmldocid` fences from those projects.

## Referenced from

- `docs/.../tutorials/beyond-basics/connect-roslyn.md`
