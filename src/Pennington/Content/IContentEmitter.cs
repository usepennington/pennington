namespace Pennington.Content;

using System.Collections.Immutable;

/// <summary>
/// Emits dynamically generated files into the build output. Smaller surface
/// than <see cref="IContentService"/> — implement this for sources that only
/// contribute build artifacts (e.g. llms.txt, robots.txt) and have nothing
/// to say about routing, discovery, or search.
/// </summary>
public interface IContentEmitter
{
    /// <summary>Files to write into the build output directory.</summary>
    Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync();
}