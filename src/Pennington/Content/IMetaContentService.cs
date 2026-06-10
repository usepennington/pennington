namespace Pennington.Content;

/// <summary>
/// Marks an <see cref="IContentService"/> whose output is DERIVED from the other registered content
/// services — taxonomy axes, paginated listings, social-card routes, and the like — rather than from
/// its own source files. When such a service walks its siblings it must skip every other meta-service,
/// itself included, or two meta-services (or a transient self-registration whose reference-equality
/// self-check never matches) would recurse without end. Filter the sibling set with
/// <c>ContentServiceExtensions.SourceServices</c> before walking it.
/// </summary>
public interface IMetaContentService
{
}
