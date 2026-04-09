namespace Pennington.Generation;

using Pennington.Routing;

public sealed class OutputOptions
{
    public required FilePath OutputDirectory { get; init; }
    public UrlPath BaseUrl { get; init; } = new("/");
    public bool CleanOutput { get; init; } = true;

    public static OutputOptions FromArgs(string[] args)
    {
        var baseUrl = args.Length > 1 ? new UrlPath(args[1]) : new UrlPath("/");
        var outputDir = args.Length > 2 ? new FilePath(args[2]) : new FilePath("output");

        return new OutputOptions
        {
            OutputDirectory = outputDir,
            BaseUrl = baseUrl
        };
    }
}
