namespace GettingStartedMinimalSiteExample;

using Pennington.FrontMatter; // [!code ++]
using Pennington.Infrastructure; // [!code ++]

/// <summary>
/// Stage 2 — register Pennington in the service container and point it at a
/// markdown folder. Middleware wiring arrives in <see cref="Stage3"/>. Tutorial
/// prose extracts the body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>.
/// This class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>Register Pennington services on a bare web host.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddPennington(penn => // [!code ++]
        { // [!code ++]
            penn.SiteTitle = "My First Pennington Site"; // [!code ++]
            penn.ContentRootPath = "Content"; // [!code ++]

            penn.AddMarkdownContent<DocFrontMatter>(md => // [!code ++]
            { // [!code ++]
                md.ContentPath = "Content"; // [!code ++]
                md.BasePageUrl = "/"; // [!code ++]
            }); // [!code ++]
        }); // [!code ++]

        var app = builder.Build();

        await app.RunAsync();
    }
}