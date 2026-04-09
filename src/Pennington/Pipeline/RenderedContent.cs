namespace Pennington.Pipeline;

using System.Collections.Immutable;
using Pennington.Search;

public record RenderedContent(
    string Html,
    OutlineEntry[] Outline,
    ImmutableList<Tag> Tags,
    ImmutableList<CrossReference> CrossReferences,
    SearchIndexDocument? SearchDocument,
    SocialMetadata? Social
);
