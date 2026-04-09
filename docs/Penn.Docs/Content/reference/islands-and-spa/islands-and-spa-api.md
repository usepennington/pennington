---
title: "Islands and SPA API"
description: "Reference for IIslandRenderer, RazorIslandRenderer<T>, ComponentRenderer, SpaPageDataService, SpaEnvelope/SpaEnvelopeDto, SpaNavigationOptions, RenderContext, data-spa-* HTML attributes, client-side JS events, and loading strategies"
uid: "penn.reference.islands-and-spa-api"
order: 10
---

Document the full SPA and islands API surface. `IIslandRenderer`: IslandName (string), `RenderAsync(route, context)` returns string. `RazorIslandRenderer<TComponent>`: abstract `BuildParametersAsync(route, context)` returns `Dictionary<string, object?>`. `ComponentRenderer`: `RenderToStringAsync<T>(parameters)`. `SpaPageDataService`: `GetPageDataAsync(slug)` returns `SpaEnvelope`. `SpaEnvelope` and `SpaEnvelopeDto`: Title, Description, Outline, Islands dictionary, Diagnostics, Reload flag. `SpaNavigationOptions`: DataPath (default `/_spa-data`). `RenderContext` record. Document the `data-spa-*` HTML attributes: `data-spa-island="name"`, `data-spa-loading="keep|skeleton|clear"`, `data-spa-skeleton-for="name"`. Document client-side JavaScript events: `spa:before-navigate`, `spa:commit`. Document `AddSpaNavigation()` and `UseSpaNavigation()` extension methods.
