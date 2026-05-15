namespace Pennington.DocSite;

/// <summary>
/// Marker resolved by the DocSite chrome and blog pages to know whether the blog is
/// active. The blog activates only when markdown articles exist under the content
/// project's <c>blog</c> folder at startup.
/// </summary>
/// <param name="Enabled">True when the blog content folder contains at least one post.</param>
public sealed record BlogFeature(bool Enabled);
