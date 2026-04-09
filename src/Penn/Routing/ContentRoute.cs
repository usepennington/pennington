namespace Pennington.Routing;

public sealed record ContentRoute
{
    public required UrlPath CanonicalPath { get; init; }
    public required FilePath OutputFile { get; init; }
    public FilePath? SourceFile { get; init; }
    public string Locale { get; init; } = "";

    /// <summary>True when this route serves default-locale content as a fallback for a missing translation.</summary>
    public bool IsFallback { get; init; }

    public UrlPath WithBaseUrl(UrlPath baseUrl) => baseUrl / CanonicalPath;
    public UrlPath AbsoluteUrl(UrlPath canonicalBase) => canonicalBase / CanonicalPath;
    public bool IsDefaultLocale => string.IsNullOrEmpty(Locale);
}
