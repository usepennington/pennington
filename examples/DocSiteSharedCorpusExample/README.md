# DocSiteSharedCorpusExample

A DocSite that ships no content of its own. It mounts the shared **Bramble**
corpus at `examples/_shared/Bramble/Content` by pointing `DocSiteOptions.ContentRootPath`
at it with a relative path. The four Diátaxis folders become areas; the corpus's
`blog/` folder auto-activates the DocSite blog.

This is the reference for the "point an example at the shared corpus" pattern, and
a realistic site-at-scale host for exercising navigation, heading-level search,
the sitemap, RSS, and llms.txt against ~100 documents.

## Concepts

- Mounting a shared content tree via a relative `ContentRootPath` (`../_shared/Bramble/Content`)
- Mapping Diátaxis folders to DocSite areas
- Blog auto-activation from a `blog/` folder inside the content root
- Reusing one corpus instead of bundling per-example markdown

## Notes

- The content is the fictional Bramble corpus — see `examples/_shared/Bramble/README.md`.
- The `<Watch>` include in the `.csproj` points at the shared folder so dev-time
  live reload picks up edits there.

## Referenced from

- Not referenced by docs pages — this is a fixture/scale host, not a tutorial surface.
