namespace Pennington.Generation;

using Pennington.Routing;

public enum LinkType
{
    Internal,
    External,
    Anchor,
    Image
}

public record BrokenLink(ContentRoute SourcePage, string Url, LinkType Type, string Reason);
