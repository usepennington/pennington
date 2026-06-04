namespace Pennington.BeyondCookFormat.Tests;

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

/// <summary>
/// Boots the <c>BeyondCookFormatExample</c> host in-memory and asserts that the custom <c>.cook</c>
/// format is discovered, parsed, and rendered through the same pipeline as markdown — the end-to-end
/// proof of the multi-format content seam.
/// </summary>
public sealed class CookFormatTests : IClassFixture<CookExampleFactory>
{
    private readonly CookExampleFactory _factory;

    public CookFormatTests(CookExampleFactory factory) => _factory = factory;

    [Fact]
    public async Task Recipe_RendersThroughTheCookFormat()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/recipes/chicken-piccata/", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        html.ShouldContain("Chicken Paccata");          // title from CookFrontMatter
        html.ShouldContain("Serves 4");                 // mapped front-matter field
        html.ShouldContain("capers");                   // a parsed Cooklang ingredient
        html.ShouldContain("class=\"ingredient\"");     // CookContentRenderer output
    }

    [Fact]
    public async Task MarkdownLandingPage_RendersAlongsideCookRecipes()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        html.ShouldContain("Cook Format Example");          // markdown page title
        html.ShouldContain("/recipes/chicken-piccata/");    // link into a cook recipe
    }

    [Fact]
    public async Task UnknownRecipe_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/recipes/not-a-real-recipe/", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}

/// <summary>
/// In-memory host factory for the example. Points the content root at the example project directory so
/// its <c>Content/</c> and <c>recipes/</c> folders resolve, mirroring a real <c>dotnet run</c>.
/// </summary>
public sealed class CookExampleFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var exampleDir = Path.Combine(FindRepoRoot(), "examples", "BeyondCookFormatExample");
        builder.UseContentRoot(exampleDir);
        Directory.SetCurrentDirectory(exampleDir);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Pennington.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new DirectoryNotFoundException(
                $"Could not locate Pennington.slnx walking up from {AppContext.BaseDirectory}.");
    }
}
