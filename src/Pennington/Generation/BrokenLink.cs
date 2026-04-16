namespace Pennington.Generation;

using Routing;

/// <summary>Classifies a broken link discovered during build-time link verification.</summary>
public enum LinkType
{
    /// <summary>Link to another page within the site.</summary>
    Internal,
    /// <summary>Link to an external origin outside the site.</summary>
    External,
    /// <summary>In-page anchor (fragment) link.</summary>
    Anchor,
    /// <summary>Image (or other media) reference.</summary>
    Image,
}

/// <summary>
/// Record of a link that failed verification during build.
/// </summary>
/// <param name="SourcePage">Route of the page that contained the broken link.</param>
/// <param name="Url">Target URL that could not be resolved.</param>
/// <param name="Type">Kind of link (internal, external, anchor, image).</param>
/// <param name="Reason">Human-readable reason the link failed verification.</param>
public record BrokenLink(ContentRoute SourcePage, string Url, LinkType Type, string Reason);