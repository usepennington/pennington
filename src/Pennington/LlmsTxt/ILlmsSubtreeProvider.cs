namespace Pennington.LlmsTxt;

using System.Collections.Immutable;

/// <summary>
/// Optional capability for a <see cref="Content.IContentService"/> to surface
/// subtree declarations discovered during its own scan (for example, <c>_meta.yml</c>
/// sidecars with an <c>llms</c> block under a markdown content tree).
/// </summary>
public interface ILlmsSubtreeProvider
{
    /// <summary>Returns the subtrees declared by this provider's content.</summary>
    Task<ImmutableList<LlmsSubtree>> GetLlmsSubtreesAsync();
}