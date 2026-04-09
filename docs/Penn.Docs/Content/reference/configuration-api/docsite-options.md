---
title: "DocSiteOptions"
description: "Complete reference for DocSiteOptions: all properties including content areas, Roslyn integration, header/footer content, social images, fonts, font preloads, routing assemblies, localization callback, and the ContentArea record"
uid: "penn.reference.docsite-options"
order: 20
---

Table-format reference for every `DocSiteOptions` property: name, type, default value, and description. Include the `ContentArea` record definition and its properties. Document what `AddDocSite` registers (Penn core, MonorailCSS, SPA navigation, ComponentRenderer, DocSiteArticleSlotRenderer island, llms.txt) so users know the full DI footprint. Document the `UseDocSite` middleware order (locale routing, antiforgery, static files, Razor components, MonorailCSS, SPA navigation, Penn core). Use `:xmldocid` code blocks where possible to keep the reference in sync with source code.
