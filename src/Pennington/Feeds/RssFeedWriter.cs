namespace Pennington.Feeds;

using System.Xml.Linq;

/// <summary>
/// Serializes a set of <see cref="RssFeedItem"/> to an RSS 2.0 document for the
/// <c>/rss.xml</c> endpoint. Items without a publish date are dropped; the rest
/// are ordered newest-first and their links composed against the feed base URL.
/// </summary>
public static class RssFeedWriter
{
    /// <summary>Builds the RSS 2.0 XML for a feed.</summary>
    /// <param name="siteTitle">Channel title.</param>
    /// <param name="siteDescription">Channel description.</param>
    /// <param name="canonicalBaseUrl">Absolute site base; when null/empty, links stay site-relative and the atom self-link is omitted.</param>
    /// <param name="items">Feed entries; each <see cref="RssFeedItem.Url"/> is a site-relative canonical path.</param>
    public static string WriteXml(
        string siteTitle,
        string siteDescription,
        string? canonicalBaseUrl,
        IEnumerable<RssFeedItem> items)
    {
        var canonicalBase = canonicalBaseUrl?.TrimEnd('/') ?? string.Empty;
        XNamespace atom = "http://www.w3.org/2005/Atom";

        var channel = new XElement("channel",
            new XElement("title", siteTitle),
            new XElement("link", string.IsNullOrEmpty(canonicalBase) ? "/" : canonicalBase + "/"),
            new XElement("description", siteDescription));

        if (!string.IsNullOrEmpty(canonicalBase))
        {
            channel.Add(new XElement(atom + "link",
                new XAttribute("href", canonicalBase + "/rss.xml"),
                new XAttribute("rel", "self"),
                new XAttribute("type", "application/rss+xml")));
        }

        var ordered = items
            .Where(i => i.PublishDate.HasValue)
            .OrderByDescending(i => i.PublishDate!.Value);

        foreach (var item in ordered)
        {
            var url = string.IsNullOrEmpty(canonicalBase)
                ? item.Url.Value
                : canonicalBase + item.Url.Value;

            var entry = new XElement("item",
                new XElement("title", item.Title),
                new XElement("link", url),
                new XElement("guid", new XAttribute("isPermaLink", "true"), url));

            if (!string.IsNullOrEmpty(item.Description))
            {
                entry.Add(new XElement("description", item.Description));
            }

            if (item.PublishDate.HasValue)
            {
                entry.Add(new XElement("pubDate", item.PublishDate.Value.ToUniversalTime().ToString("r")));
            }

            if (!string.IsNullOrEmpty(item.Author))
            {
                entry.Add(new XElement("author", item.Author));
            }

            channel.Add(entry);
        }

        var rss = new XElement("rss",
            new XAttribute("version", "2.0"),
            new XAttribute(XNamespace.Xmlns + "atom", atom.NamespaceName),
            channel);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), rss);
        return doc.Declaration + Environment.NewLine + doc;
    }
}
