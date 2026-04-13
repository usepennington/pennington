namespace Pennington.StructuredData;

using System.Text.Encodings.Web;
using System.Text.Json;

/// <summary>
/// Serializes JSON-LD structured data types to JSON strings safe for embedding
/// in HTML script tags.
/// </summary>
public static class JsonLdSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Serialize an Article schema to JSON-LD.</summary>
    public static string SerializeArticle(JsonLdArticle article)
    {
        var dict = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Article",
            ["headline"] = article.Headline,
            ["url"] = article.Url,
        };

        if (article.Description is not null)
            dict["description"] = article.Description;

        if (article.DatePublished is { } date)
            dict["datePublished"] = date.ToString("yyyy-MM-ddTHH:mm:ssZ");

        if (article.AuthorName is not null)
            dict["author"] = new Dictionary<string, object>
            {
                ["@type"] = "Person",
                ["name"] = article.AuthorName,
            };

        return EscapeForScriptTag(JsonSerializer.Serialize(dict, Options));
    }

    /// <summary>Serialize a BreadcrumbList schema to JSON-LD. Returns null when the list is empty.</summary>
    public static string? SerializeBreadcrumbList(JsonLdBreadcrumbList breadcrumbs)
    {
        if (breadcrumbs.Items.Count == 0)
            return null;

        var elements = new List<Dictionary<string, object?>>();
        foreach (var item in breadcrumbs.Items)
        {
            var element = new Dictionary<string, object?>
            {
                ["@type"] = "ListItem",
                ["position"] = item.Position,
                ["name"] = item.Name,
            };

            if (item.Url is not null)
                element["item"] = item.Url;

            elements.Add(element);
        }

        var dict = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = elements,
        };

        return EscapeForScriptTag(JsonSerializer.Serialize(dict, Options));
    }

    /// <summary>Serialize a WebSite schema to JSON-LD.</summary>
    public static string SerializeWebSite(JsonLdWebSite webSite)
    {
        var dict = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "WebSite",
            ["name"] = webSite.Name,
            ["url"] = webSite.Url,
        };

        if (webSite.Description is not null)
            dict["description"] = webSite.Description;

        return EscapeForScriptTag(JsonSerializer.Serialize(dict, Options));
    }

    /// <summary>
    /// Escapes sequences that would prematurely close a script tag when the JSON
    /// is embedded inline in HTML.
    /// </summary>
    private static string EscapeForScriptTag(string json) =>
        json.Replace("</", "<\\/");
}