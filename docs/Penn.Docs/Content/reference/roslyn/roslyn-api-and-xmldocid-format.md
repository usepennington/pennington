---
title: "Roslyn API and XmlDocId Format"
description: "Reference for RoslynOptions, ProjectFilter, AddPennRoslyn registration, ICodeBlockPreprocessor, all code block modifiers (:xmldocid, :path, :xmldocid-diff), and the XmlDocId format specification (prefixes, generics, nested types, constructors, parameters)"
uid: "penn.reference.roslyn-api-and-xmldocid-format"
order: 10
---

Two-part reference page. Part 1 — Roslyn API: `RoslynOptions` (SolutionPath, ProjectFilter), `ProjectFilter` (IncludedProjects, ExcludedProjects hash sets), `AddPennRoslyn` extension method signature, `ICodeBlockPreprocessor` interface (Priority, TryProcess), `CodeBlockPreprocessResult` record. Document each code block modifier: `:xmldocid` (full symbol source), `:xmldocid,bodyonly` (method body only), `:xmldocid-diff` (two-symbol diff), `:xmldocid-diff,bodyonly`, `:path` (file by path). Part 2 — XmlDocId Format Specification: prefix conventions (`N:` namespace, `T:` type, `M:` method, `P:` property, `F:` field, `E:` event), generic type notation (backtick + arity), nested types (`+` separator), method signatures with parameter types, constructors (`#ctor`, `#cctor`), generic methods (double backtick), arrays, pointers, ref/out parameters. Include a quick-reference table of common Penn type XmlDocIds.
