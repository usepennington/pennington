---
title: "Routing Types"
description: "Reference for UrlPath (operators, matching, normalization), FilePath (operators, extension, file name), ContentRoute (CanonicalPath, OutputFile, SourceFile, Locale, IsFallback), and ContentRouteFactory"
uid: "penn.reference.routing-types"
order: 10
---

Document the three core routing value types. `UrlPath`: the `/` operator for joining, `Matches()` (case-insensitive, normalizes trailing slashes and `.html`), `EnsureLeadingSlash()`, `EnsureTrailingSlash()`, `RemoveLeadingSlash()`, `RemoveTrailingSlash()`, implicit conversion from string. `FilePath`: the `/` operator, `Extension` property, `FileName` property, implicit conversion from string. `ContentRoute`: `CanonicalPath` (UrlPath), `OutputFile` (FilePath), `SourceFile` (FilePath), `Locale` (string), `IsFallback` (bool). Document `ContentRouteFactory` and its static methods for constructing routes from file paths and Razor page directives. This is a type reference — show signatures and behavior, not usage patterns.
