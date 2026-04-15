namespace GettingStartedMinimalSiteExample;

using Pennington.FrontMatter;
using Pennington.Infrastructure;

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

        builder.Services.AddPennington(penn =>
        {
            penn.SiteTitle = "My First Pennington Site";
            penn.ContentRootPath = "Content";

            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "/";
            });
        });

        var app = builder.Build();

        await app.RunAsync();
    }
}