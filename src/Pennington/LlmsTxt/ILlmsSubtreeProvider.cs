namespace Pennington.LlmsTxt;

using System.Collections.Immutable;

/// <summary>
/// Optional capability for a <see cref="Pennington.Content.IContentService"/> to surface
/// subtree declarations discovered during its own scan (for example, <c>_llms.yaml</c>
/// sidecars under a markdown content tree).
/// </summary>
public interface ILlmsSubtreeProvider
{
    /// <summary>Returns the subtrees declared by this provider's content.</summary>
    Task<ImmutableList<LlmsSubtree>> GetLlmsSubtreesAsync();
}