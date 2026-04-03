namespace Penn.Generation;

using Penn.Routing;

public enum LinkType
{
    Internal,
    External,
    Anchor,
    Image
}

public record BrokenLink(ContentRoute SourcePage, string Url, LinkType Type, string Reason);
