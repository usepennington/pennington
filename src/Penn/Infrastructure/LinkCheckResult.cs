namespace Pennington.Infrastructure;

using Pennington.Generation;
using Pennington.Routing;

// Case types
public record ValidLink(ContentRoute SourcePage, string Url);
public record BrokenLinkResult(ContentRoute SourcePage, string Url, LinkType Type, string Reason);
public record ExternalLink(ContentRoute SourcePage, string Url);

// The union
public union LinkCheckResult(ValidLink, BrokenLinkResult, ExternalLink);
