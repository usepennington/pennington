---
title: "Static Generation"
description: "Reference for OutputGenerationService 9-phase process, OutputOptions (OutputDirectory, BaseUrl, CleanOutput, FromArgs), BuildReport and BuildReportBuilder, and the CLI argument format"
uid: "penn.reference.static-generation"
order: 10
---

Document `OutputGenerationService` and its 9 phases in order: (1) collect content pages from all `IContentService` implementations, (2) discover MapGet routes from endpoint data source, (3) clear/create output directory, (4) copy static assets (content files, wwwroot, RCL assets), (5) create dynamic files (search index, etc.), (6) fetch content pages via HTTP (parallel), (7) fetch MapGet routes last (CSS must come after HTML for class collection), (8) generate 404.html from fallback route, (9) verify internal links. Document `OutputOptions`: OutputDirectory (FilePath), BaseUrl (UrlPath, default "/"), CleanOutput (bool, default true), `FromArgs(string[])` static factory. Document `BuildReport`: GeneratedPages, SkippedPages, FailedPages, Diagnostics, BrokenLinks lists. Document `BuildReportBuilder`. Document the CLI argument format: `dotnet run build [baseUrl] [outputDir]`.
