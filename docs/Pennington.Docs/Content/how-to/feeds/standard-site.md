---
title: "Verify your site on the AT Protocol network"
description: "Configure Standard Site to publish the well-known files and site.standard.* head links that prove your site backs an AT Protocol publication, so shared links render rich Bluesky cards."
uid: how-to.feeds.standard-site
order: 5
sectionLabel: "Feeds & Indexes"
tags: [standard-site, atproto, discovery, verification, configuration]
isDraft: true
---

<!--
DRAFT — hidden from the production build (isDraft), still renders in dev.

Why: the publication-level path (one-time record → rich Bluesky card) is real and
nearly zero-friction, but the per-document path makes authors hand-create a
site.standard.document record and paste its rkey into front matter for every post.
That manual two-system loop won't be sustained, and it sits on the least-mature
benefit (document-level following in reader apps that barely exist yet).

Un-draft (flip isDraft to false) once there's a publish step that writes the
document record to the PDS and feeds the rkey back automatically, OR once the page
is reframed around the one-time publication setup as THE path and per-document
records are clearly marked optional / forward-looking.
-->

Share a link to your site on Bluesky and it shows a link preview. Turn this on and that preview becomes a card the AT Protocol network attributes to *your publication* — recognized as content you own, with your name and branding — instead of a generic preview of an anonymous page.

The bigger payoff is what *publication* means on the network. The AT Protocol is the open social layer behind Bluesky, and on it a publication is something readers can follow, the same way they follow an account: reader apps can surface your posts and let people subscribe to them. And because the canonical metadata lives in records you own rather than on any single host, your audience and identity travel with you if you ever move hosts. Verification is what lets the network connect your web pages to that publication.

[Standard Site](https://standard.site) defines the shared format that makes this work, and it comes in two pieces that live in different places. The first is a set of records — one for the *publication* (your site as a whole) and one for each *document* (a post or page) — that hold the canonical metadata the network reads. These aren't your site's content: they live in your AT Protocol account, and you author them in a Standard Site editor such as [standard.horse](https://standard.horse), the way you'd manage any other data on your account. Pennington never creates or edits them.

Verification is the second piece, and it's Pennington's half. From your built site it publishes the proof that ties the site to those records — a `/.well-known/` file naming your publication and the `site.standard.*` `<link>` tags a card reader checks — so the network trusts that this site is the web home of that publication. You own the records; Pennington vouches that this site backs them.

## Before you begin

You need two values from your AT Protocol account, plus a Pennington site:

- **A DID** — your account's permanent identifier, in the form `did:plc:…` (or `did:web:…` for a domain-based identity). It is the same account that owns your Bluesky handle; find it in your Bluesky account settings, or resolve it from your handle. This is the `Did` value below.
- **A publication record key (rkey)** — create a `site.standard.publication` record in a Standard Site editor such as [standard.horse](https://standard.horse). A *record key* is the trailing segment of a record's `at://` URI that names one specific record in your repository (`at://{Did}/site.standard.publication/{rkey}`); the editor shows it when you save the publication. This is the `PublicationRkey` value below.
- **A working Pennington site** (see <xref:tutorials.getting-started.first-site> if not).

## Configure the publication

Set `StandardSiteOptions` with your DID and publication record key. On a bare host, assign it on `PenningtonOptions`; the `DocSiteOptions` and `BlogSiteOptions` templates forward a `StandardSite` property the same way they forward social cards.

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    SiteDescription = "Project documentation",
    StandardSite = new StandardSiteOptions
    {
        Did = "did:plc:abc123",
        PublicationRkey = "3lk2hf4xa2b2c",
    },
});
```

`Did` and `PublicationRkey` are the only required values. When either is blank the feature emits nothing — never a half-formed link pointing at a record that does not exist.

```csharp:symbol,signatures
src/Pennington/StandardSite/StandardSiteOptions.cs > StandardSiteOptions
```

With that in place, the build bakes `/.well-known/site.standard.publication` (the bare publication AT-URI, `text/plain`) into the output and the dev server serves the same bytes, while every page head carries `<link rel="site.standard.publication" href="at://…">`.

## Options

### Link a page to its `site.standard.document` record

Once a page has its own `site.standard.document` record, set `atprotoRkey` in the page's front matter. Pages that carry it emit a second head link, `<link rel="site.standard.document" href="at://…">`; pages without it emit only the site-wide publication link.

```yaml
---
title: My first post
atprotoRkey: 3lk2post9x8y7
---
```

The bundled `BlogPostFrontMatter` and `BlogSiteFrontMatter` records implement `IStandardSiteDocument`, so `atprotoRkey` is recognized with no extra wiring. Records that do not carry a date get publication-level verification only — a per-document record is keyed to published content.

```csharp:symbol
src/Pennington/FrontMatter/Capabilities.cs > IStandardSiteDocument
```

### Map the document rkey from custom front matter

To source the per-page rkey from somewhere other than `IStandardSiteDocument` — a custom front-matter record, or metadata you compute — set `DocumentRkeyResolver`. It receives the resolved content record and returns the rkey, or `null` for pages with no document.

```csharp
StandardSite = new StandardSiteOptions
{
    Did = "did:plc:abc123",
    PublicationRkey = "3lk2hf4xa2b2c",
    DocumentRkeyResolver = record =>
        (record.Metadata as MyFrontMatter)?.RecordKey,
};
```

### Serve a publication under a sub-path

When the publication is verified for a path below the domain root rather than the whole domain, set `PublicationPath` with a leading slash and no trailing slash. It is appended to the well-known suffix, so `/blog` serves the proof at `/.well-known/site.standard.publication/blog`.

```csharp
PublicationPath = "/blog",
```

### Claim the domain as your atproto handle

Setting `EmitAtprotoDid = true` also emits `/.well-known/atproto-did` (the bare DID), which makes the site's domain double as your Bluesky handle. This defaults to `false` because it is a stronger, separate claim than publication verification: a mismatched DID can break an existing domain handle set up through DNS. Turn it on only when you intend the domain to *be* your handle.

```csharp
EmitAtprotoDid = true,
```

### Drop the site-wide publication link

`EmitPublicationLink` defaults to `true` — it is the head tag that makes a shared link render a rich Bluesky card, and the point of Tier 1 verification. Set it to `false` to keep the well-known file but suppress the head link.

## Result

A configured site exposes the verification file and head links. The well-known file is the bare AT-URI as `text/plain`:

```text
at://did:plc:abc123/site.standard.publication/3lk2hf4xa2b2c
```

A blog post that declares `atprotoRkey` carries both links in its head:

```html
<link rel="site.standard.publication" href="at://did:plc:abc123/site.standard.publication/3lk2hf4xa2b2c" data-head>
<link rel="site.standard.document" href="at://did:plc:abc123/site.standard.document/3lk2post9x8y7" data-head>
```

A page without an rkey, and the home page, carry only the publication link.

> [!NOTE]
> The well-known file has no extension, so a pure-static host serves it with whatever default MIME type it assigns to extensionless files. GitHub Pages serves it as `text/plain`, which Standard Site accepts; if you deploy elsewhere, confirm the host does not return `application/octet-stream`. See <xref:how-to.deployment.github-pages> for the deployment path.

## Verify

- Run `dotnet run -- diag standard-site`. It prints the resolved publication AT-URI and well-known path, then reports how many document-capable pages link to a record — and warns when a `Did` is malformed or an rkey is missing, turning a silent card failure into a build-time signal.
- Run `dotnet run` and fetch `/.well-known/site.standard.publication`. Expect the bare AT-URI with `Content-Type: text/plain`.
- View-source a post that declares `atprotoRkey` and confirm both `site.standard.*` links are present; view-source the home page and confirm only the publication link is there.
- Static build: `dotnet run -- build output` and confirm `output/.well-known/site.standard.publication` exists with the AT-URI as its content.

## Related

- Background: [The head subsystem](xref:explanation.core.head-subsystem) — how the `site.standard.*` links are composed and stamped `data-head`.
- How-to: [Add tags to the document head](xref:how-to.response-pipeline.head-contributor) — write your own discovery-band contributor.
- Reference: [Front matter key reference](xref:reference.front-matter.keys) — where `atprotoRkey` appears.
- How-to: [Deploy to GitHub Pages](xref:how-to.deployment.github-pages) — serving the well-known file from a static host.
