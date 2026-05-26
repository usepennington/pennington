namespace Pennington.Content;

using System.Collections.Immutable;

/// <summary>
/// Optional capability for a <see cref="IContentService"/> to surface
/// <see cref="FolderMetadata"/> rows discovered during its own scan (for example,
/// <c>_meta.yml</c> sidecars under a markdown content tree).
/// </summary>
public interface IFolderMetadataProvider
{
    /// <summary>Returns the folder-metadata rows declared by this provider's content.</summary>
    Task<ImmutableList<FolderMetadata>> GetFolderMetadataAsync();
}
