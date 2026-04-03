namespace Penn.Routing;

public sealed record ContentRoute
{
    public required UrlPath CanonicalPath { get; init; }
    public required FilePath OutputFile { get; init; }
    public FilePath? SourceFile { get; init; }
    public string Locale { get; init; } = "";

    public UrlPath NavigationPath => CanonicalPath.EnsureTrailingSlash();
    public UrlPath WithBaseUrl(UrlPath baseUrl) => baseUrl / CanonicalPath;
    public UrlPath AbsoluteUrl(UrlPath canonicalBase) => canonicalBase / CanonicalPath;
    public bool IsDefaultLocale => string.IsNullOrEmpty(Locale);
}
