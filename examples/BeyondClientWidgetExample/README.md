# BeyondClientWidgetExample

A DocSite that ships one custom **client-side widget** — an image-gallery
lightbox — by composing the seams a static engine gives you: a CDN script, the
`<head>` injection seam, and a server-rendered Mdazor component the script
enhances.

## What it teaches

Pennington renders every page on the server with no client hydration. To add
browser behavior you bring your own script and attach it to the rendered HTML.
This example shows the whole loop:

- **`GalleryWidget.cs`** — `BuildDocSiteOptions()` sets
  `DocSiteOptions.AdditionalHtmlHeadContent` to inject the GLightbox stylesheet
  and script from jsDelivr plus the local `/gallery.js` init script. GLightbox
  is MIT-licensed and dependency-free; the version is pinned.
- **`Components/ImageGallery.razor`** — a server-rendered Mdazor component that
  emits the `<a class="glightbox">` / `<img>` thumbnail grid. Registered with
  `AddMdazorComponent<ImageGallery>()` in `Program.cs`.
- **`wwwroot/gallery.js`** — the client half: on load (and after each
  `spa:commit`) it calls `GLightbox({ selector: '.glightbox' })` to upgrade the
  thumbnails into a lightbox.
- **`Content/guides/image-gallery.md`** — drops `<ImageGallery ... />` into a
  markdown page.

The gallery images are the shared train set in `examples/_shared/Images`, linked
into `wwwroot/images` by the `.csproj` so they are served at dev time and copied
into the static build output.

## Run it

```
dotnet run --project examples/BeyondClientWidgetExample
```

Open `/guides/image-gallery` and click a thumbnail. To produce the static build:

```
dotnet run --project examples/BeyondClientWidgetExample -- build output
```

## Referenced from

- `how-to/rich-content/client-side-widget.md`
