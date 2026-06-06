namespace Pennington.StandardSite;

using System.Text.RegularExpressions;

/// <summary>
/// Builds and parses AT Protocol <c>at://</c> URIs of the form
/// <c>at://{did}/{collection}/{rkey}</c>. Pure string work — no PDS access.
/// </summary>
public static partial class AtUri
{
    /// <summary>Composes an <c>at://</c> URI from its parts.</summary>
    public static string Build(string did, string collection, string rkey)
        => $"at://{did}/{collection}/{rkey}";

    /// <summary>
    /// Parses an <c>at://</c> URI into its DID, collection NSID, and record key, or returns
    /// <c>null</c> when the input is not a well-formed three-part AT-URI.
    /// </summary>
    public static (string Did, string Collection, string Rkey)? Parse(string uri)
    {
        var match = AtUriPattern().Match(uri);
        return match.Success
            ? (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value)
            : null;
    }

    [GeneratedRegex(@"^at://([^/]+)/([^/]+)/(.+)$")]
    private static partial Regex AtUriPattern();
}
