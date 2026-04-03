namespace Penn.Pipeline;

using System.Collections.Immutable;
using Penn.Search;

public record RenderedContent(
    string Html,
    OutlineEntry[] Outline,
    ImmutableList<Tag> Tags,
    ImmutableList<CrossReference> CrossReferences,
    SearchIndexDocument? SearchDocument,
    SocialMetadata? Social
);
