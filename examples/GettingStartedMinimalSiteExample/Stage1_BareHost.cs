namespace GettingStartedMinimalSiteExample;

/// <summary>
/// Stage 1 — a bare ASP.NET host with no Pennington wiring yet. Tutorial
/// prose extracts the body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>.
/// This class is never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>Create a minimal web application and run it.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello from ASP.NET.");

        await app.RunAsync();
    }
}
