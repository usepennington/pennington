---
title: "The SDK you need and the union shim"
description: "Why the published packages need only the stable .NET 10 SDK, when the .NET 11 beta SDK is worth opting into, and what the union polyfill does underneath."
uid: explanation.positioning.sdk-and-the-union-shim
order: 2
sectionLabel: "Positioning"
tags: [sdk, dotnet, unions, polyfill, pipeline]
---

Pennington's source is written in C# 15 — it uses the `union` keyword for its pipeline types. A reasonable question follows: does building a site on Pennington need the C# 15 compiler and a preview .NET SDK? For the published packages, no. The stable .NET 10 SDK is enough.

## Which SDK each audience needs

Every consumer path — the DocSite template, the BlogSite template, and a host wired directly on `AddPennington` — builds against the stable .NET 10 SDK. The project file needs nothing more than `<TargetFramework>net10.0</TargetFramework>`; there is no `<LangVersion>preview</LangVersion>` to set.

The reason is that a consumer never writes the `union` keyword. Pennington's pipeline types — `ContentItem`, `ContentSource`, and the rest — are unions, but you only ever *call* methods that return them and read the case through `.Value`. Reading `.Value` is ordinary C# that compiles on .NET 10 unchanged. The preview language feature lives entirely inside Pennington's own source, not at your call sites. The [first-site tutorial](xref:tutorials.getting-started.first-site) wires a host this way, on plain `net10.0`.

## Building from source versus consuming the packages

The one place the preview SDK is still required is building Pennington itself. The library multi-targets `net10.0;net11.0`, and the `net11.0` build compiles the real `union` keyword, so the repository pins the .NET 11 preview SDK in `global.json`. That requirement belongs to the library's build, not to yours: when you reference the published `Pennington.*` packages, NuGet hands your `net10.0` project the `net10.0` build, and the preview SDK never enters the picture.

## What the union shim is

Multi-targeting is what lets one source tree serve both SDKs. On `net11.0` the C# 15 `union` keyword synthesizes each pipeline union. On `net10.0`, where that keyword does not exist, a hand-written polyfill struct stands in — same cases, same `.Value` field, same shape at every call site. The polyfill is why a `net10.0` consumer sees an API identical to a `net11.0` one. <xref:explanation.core.content-source> covers why every read goes through `.Value`, and why the polyfill is shaped to match the keyword exactly rather than take a shortcut that would diverge between the two builds.

## What the .NET 11 beta SDK buys you

Opting into the .NET 11 preview SDK and targeting `net11.0` changes one thing, and it matters to one audience: people extending the pipeline. When you branch on the `ContentItem` or `ContentSource` cases in your own code, the `net11.0` build lets you switch over the union directly, and the compiler enforces exhaustiveness — a `switch` that stops covering every case becomes a compile error that points at exactly the code a new case broke. On `net10.0` that direct match is the preview feature you are avoiding, so you read the case through `.Value` and switch over that instead — `item.Value switch { ParsedItem p => … }` — which compiles cleanly but gives up the exhaustiveness check. Reading `.Value` is the portable form, and it is the one the templates and examples use so they build on either SDK.

That safety net is the whole of the upgrade. It is invisible to anyone who is not extending the pipeline, which is why stable .NET 10 is the default the templates and tutorials assume. The tradeoff for a pipeline author — a preview SDK in exchange for a compiler guarantee — is the one worth weighing; see <xref:explanation.core.content-pipeline> for why the unions are exhaustive in the first place, and <xref:how-to.content-services.custom-content-service> for the extension recipe.

## Further reading

- Explanation: [Why ContentSource is a union](xref:explanation.core.content-source) — why every consumer goes through `.Value`, and why the polyfill matches the keyword.
- Explanation: [The content pipeline and union types](xref:explanation.core.content-pipeline) — the pipeline unions and why exhaustiveness matters when you extend them.
- Tutorial: [Create your first Pennington site](xref:tutorials.getting-started.first-site) — a consumer host wired on stable .NET 10.
