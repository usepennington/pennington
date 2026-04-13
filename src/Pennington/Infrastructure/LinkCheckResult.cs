namespace Pennington.Infrastructure;

using Generation;
using Routing;

// Case types
public record ValidLink(ContentRoute SourcePage, string Url);
public record BrokenLinkResult(ContentRoute SourcePage, string Url, LinkType Type, string Reason);
public record ExternalLink(ContentRoute SourcePage, string Url);

// The union
public union LinkCheckResult(ValidLink, BrokenLinkResult, ExternalLink);