namespace Pennington.Content;

/// <summary>
/// Metadata for a content folder discovered from a <c>_meta.yml</c> sidecar.
/// Lets a folder declare its own display title, its position in the parent's
/// nav level, and (optionally) its participation in the <c>llms.txt</c> sidecar.
/// </summary>
/// <param name="FolderUrlPrefix">Canonical URL prefix in <c>/foo/bar/</c> form (always leading and trailing slash) identifying the folder this metadata applies to.</param>
/// <param name="Title">Display title for the folder; overrides <c>FormatSectionTitle</c> and any <c>index.md</c> title when set.</param>
/// <param name="Order">Sort order of the folder within its parent level; overrides emergent <c>min(children)</c> and any <c>index.md</c> order when set.</param>
/// <param name="LlmsDescription">When non-null, opts the subtree into <c>llms.txt</c> generation; the value is the subtree blurb.</param>
public sealed record FolderMetadata(
    string FolderUrlPrefix,
    string? Title,
    int? Order,
    string? LlmsDescription);
