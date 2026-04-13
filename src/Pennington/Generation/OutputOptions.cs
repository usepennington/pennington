namespace Pennington.Generation;

using Routing;

public sealed class OutputOptions
{
    public required FilePath OutputDirectory { get; init; }
    public UrlPath BaseUrl { get; init; } = new("/");
    public bool CleanOutput { get; init; } = true;

    public static OutputOptions FromArgs(string[] args)
    {
        // OutputOptions is only meaningful for the "build" CLI verb — the same
        // sentinel RunOrBuildAsync uses. In any other invocation (dev run,
        // tests, tooling) the positional args belong to something else (xunit
        // runner, Kestrel flags, …) and must not be interpreted as a base URL
        // or output directory. Previously we blindly read args[1]/args[2],
        // which under `dotnet test` pulled a temp path into BaseUrl and
        // corrupted every rewritten <a href> in integration tests.
        if (args.Length == 0 || !args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
        {
            return new OutputOptions { OutputDirectory = new FilePath("output") };
        }

        var baseUrl = args.Length > 1 ? new UrlPath(args[1]) : new UrlPath("/");
        var outputDir = args.Length > 2 ? new FilePath(args[2]) : new FilePath("output");

        return new OutputOptions
        {
            OutputDirectory = outputDir,
            BaseUrl = baseUrl,
        };
    }
}