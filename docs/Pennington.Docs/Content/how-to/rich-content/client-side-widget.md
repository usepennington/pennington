---
title: "Ship a custom client-side widget"
description: "Add browser behavior to a static Pennington site by composing a server-rendered Mdazor component, your own script, and the head seam that loads a CDN library — built here as an image-gallery lightbox."
uid: how-to.rich-content.client-side-widget
order: 6
sectionLabel: "Rich Content"
tags: [client-side, javascript, mdazor, components, spa]
---

Pennington renders every page on the server in a single pass — there is no client-side hydration. To add interactive browser behavior (a lightbox, a chart, a copy-to-clipboard button), you ship your own script and attach it to the server-rendered HTML.

This guide builds an image-gallery lightbox from three parts: a server-rendered component that emits the markup, a browser script that enhances it, and the head content option that loads both your script and the third-party library. The worked library is [GLightbox](https://github.com/biati-digital/glightbox) (MIT-licensed, dependency-free), but the pattern is the same for any library that scans the DOM and upgrades matching elements — the bundled Mermaid support (<xref:how-to.rich-content.diagrams>) follows it too.

## Before you begin

- A DocSite (`AddDocSite`) or BlogSite host — this example is a DocSite. On a bare `AddPennington` host the only difference is the head content: inject the tags through your own layout's `<head>` or a response processor that inserts before `</body>` (see <xref:how-to.response-pipeline.response-processor>).
- Familiarity with the library you are wrapping. This page covers the wiring, not GLightbox itself.
- For a complete, running setup, see `examples/BeyondClientWidgetExample`; the sections below embed each of its files where they apply.

## Render the markup on the server

Write an Mdazor component that emits the HTML the script will later find and upgrade. The component is plain server-side Razor — it renders thumbnails wrapped in `<a class="glightbox">` anchors and nothing more. The lightbox behavior is added entirely by the script in the next step.

```razor:symbol
examples/BeyondClientWidgetExample/Components/ImageGallery.razor
```

Register the component so it is usable as a tag in markdown. `AddMdazorComponent<T>()` is the only DI line needed; the registry resolves the tag at render time.

```csharp:symbol
examples/BeyondClientWidgetExample/Program.cs
```

Only primitive attributes bind from markdown, so the image list arrives as one comma-separated string and the component derives a caption from each file name. For the binding rules and the structured-data workarounds, see <xref:how-to.rich-content.ui-components-in-markdown>.

## Write the browser script

The script runs in the browser, finds the server-rendered anchors, and hands them to the library. Keep the initializer idempotent — it runs once on the first load and again after every in-site navigation (covered under [Survive SPA navigation](#survive-spa-navigation) below).

```javascript:symbol
examples/BeyondClientWidgetExample/wwwroot/gallery.js
```

Put the script in `wwwroot`, where the host serves it at `/gallery.js` and the static build copies it to the output.

## Load the library and your script

`DocSiteOptions.AdditionalHtmlHeadContent` is a raw HTML string rendered inside every page's `<head>` — the place for the library's stylesheet and script plus your own. Load the library first, then your script. Both `<script>` tags use `defer`, so they execute in document order: the library defines its global before your script calls it.

```csharp:symbol,bodyonly
examples/BeyondClientWidgetExample/GalleryWidget.cs > GalleryWidget.BuildGalleryHeadContent
```

Pin the library to a version so the build is reproducible, and assign the fragment to the head content option on the options record:

```csharp:symbol
examples/BeyondClientWidgetExample/GalleryWidget.cs > GalleryWidget.BuildDocSiteOptions
```

For a site that builds offline or behind a firewall, vendor the two library files into `wwwroot` and point the tags at the local copies — a CDN load fails silently otherwise.

## Display the media the widget references

Colocate the images the gallery displays under `Content`, next to the page that uses them — here, `Content/guides/assets/`. Pennington copies colocated assets to the output and the build's link checker recognizes them, so referencing them from the rendered markup keeps the build clean. See <xref:how-to.pages.images-and-assets>.

## Use it in a page

Drop the tag into any markdown page. The `Images` attribute is the comma-separated file list, `Group` ties the thumbnails into one lightbox so the arrow keys move between them, and `BasePath` (optional) is the URL prefix the files are served from.

```markdown
<ImageGallery Images="peppermint-express.png, merry-mixer.png, indigo-inchworm.png" Group="trains" />
```

## Survive SPA navigation

Pennington's SPA engine swaps page content on in-site navigation without a full reload, which affects a client widget two ways.

**Re-run your initializer.** `DOMContentLoaded` fires only on the first full load. After an in-site navigation the new page's markup arrives through a region swap, so re-bind from the `spa:commit` event — `gallery.js` above adds one listener for exactly this. See the [SPA lifecycle events](xref:reference.spa.attributes#lifecycle-events).

**Opt link-triggered widgets out of navigation.** The engine treats same-origin `<a>` clicks as navigation. Because the gallery thumbnails are links to the full image, an un-marked click would be intercepted by the engine instead of opening the lightbox. The engine automatically skips links marked `target="_blank"` or `download` (see [anchor attributes](xref:reference.spa.attributes#anchor-and-stylesheet-attributes)), so the component sets `target="_blank"` — GLightbox calls `preventDefault`, making the new tab a graceful fallback only when scripting is off.

## Verify

- Run `dotnet run --project examples/BeyondClientWidgetExample` and open `/guides/image-gallery`. Click a thumbnail — the lightbox opens. Navigate to the page through an in-site link and click again — it still opens, confirming the `spa:commit` re-init.
- Run `dotnet run --project examples/BeyondClientWidgetExample -- build output`. Confirm `output/gallery.js`, the images under `output/guides/assets/`, and the `<a class="glightbox">` markup in `output/guides/image-gallery/index.html` are all present, and that the build reports no broken links.

## Related

- How-to: <xref:how-to.rich-content.ui-components-in-markdown> — the attribute-binding rules for the Mdazor tag this widget renders.
- How-to: <xref:how-to.response-pipeline.response-processor> — inject the head/`</body>` tags on a bare `AddPennington` host instead of through `DocSiteOptions`.
- How-to: <xref:how-to.pages.images-and-assets> — where page-referenced images and shared assets live.
- Reference: <xref:reference.spa.attributes> — the SPA engine's anchor opt-outs and `spa:commit` lifecycle event.
- Background: <xref:explanation.spa.islands> — why the SPA model is server-rendered with no client hydration.
