namespace Pennington.Generation;

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