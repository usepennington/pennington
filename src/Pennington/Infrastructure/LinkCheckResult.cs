namespace Pennington.Infrastructure;

using Generation;
using Routing;

// Case types
/// <summary>A link that resolved to a known internal target.</summary>
/// <param name="SourcePage">Page that contained the link.</param>
/// <param name="Url">Link target URL.</param>
public record ValidLink(ContentRoute SourcePage, string Url);

/// <summary>A link that failed verification.</summary>
/// <param name="SourcePage">Page that contained the link.</param>
/// <param name="Url">Link target URL.</param>
/// <param name="Type">Classification of the broken link.</param>
/// <param name="Reason">Human-readable reason the link is considered broken.</param>
public record BrokenLinkResult(ContentRoute SourcePage, string Url, LinkType Type, string Reason);

/// <summary>A link that points to an external origin and was not verified by the internal checker.</summary>
/// <param name="SourcePage">Page that contained the link.</param>
/// <param name="Url">External target URL.</param>
public record ExternalLink(ContentRoute SourcePage, string Url);

// The union
/// <summary>Outcome of checking a single link during build-time verification.</summary>
public union LinkCheckResult(ValidLink, BrokenLinkResult, ExternalLink);