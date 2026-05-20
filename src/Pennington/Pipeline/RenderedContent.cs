namespace Pennington.Pipeline;

using System.Collections.Immutable;
using Search;

/// <summary>Output produced by the render stage for a single content item.</summary>
/// <param name="Html">Rendered HTML body.</param>
/// <param name="Outline">Heading outline extracted from the content.</param>
/// <param name="Tags">Tags associated with the content.</param>
/// <param name="CrossReferences">Cross-reference targets defined by the content.</param>
/// <param name="SearchDocument">Optional search index document for this page.</param>
/// <param name="Social">Optional social/Open Graph metadata for this page.</param>
public record RenderedContent(
    string Html,
    OutlineEntry[] Outline,
    ImmutableList<Tag> Tags,
    ImmutableList<CrossReference> CrossReferences,
    SearchIndexDocument? SearchDocument,
    SocialMetadata? Social
);