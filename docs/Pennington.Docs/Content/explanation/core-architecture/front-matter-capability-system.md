---
title: "The Front Matter Capability System"
description: "Why Pennington uses capability interfaces instead of inheritance or a single front matter type — covering pattern matching composition, pipeline obliviousness to unused capabilities, and YAML deserialization mechanics"
uid: "penn.explanation.front-matter-capability-system"
order: 30
---

Explain why Pennington uses capability interfaces (`IDraftable`, `ITaggable`, etc.) instead of a single large front matter type or a class hierarchy. The core insight: different content types need different metadata, but the pipeline needs to handle all content types uniformly. Pattern matching (`item.Metadata is IDraftable { IsDraft: true }`) lets pipeline stages check for capabilities they care about and ignore everything else — a stage that filters drafts doesn't need to know about tags. Contrast with the alternatives: a single fat type (every content type carries unused fields), inheritance (rigid hierarchy, diamond problem), and marker interfaces without properties (no compile-time guarantees). Discuss the YAML deserialization story: how YamlDotNet maps camelCase YAML keys to record properties, and why records (not classes) are the right choice for immutable front matter.
