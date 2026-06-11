namespace Pennington.Book.Tests;

using Microsoft.Extensions.DependencyInjection;
using Pennington.Book;
using Pennington.Book.Composition;
using Pennington.Book.Rendering;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Navigation;

public sealed class PenningtonBookExtensionsTests
{
    [Fact]
    public void Registers_options_catalog_browser_artifact_emitter_and_composer()
    {
        var services = new ServiceCollection();

        services.AddPenningtonBook(o => o.Books.Add(new BookDefinition("Guides", "/how-to/")));

        services.ShouldContain(d => d.ServiceType == typeof(BookOptions));
        services.ShouldContain(d => d.ServiceType == typeof(IDownloadLinkProvider));
        services.ShouldContain(d => d.ServiceType == typeof(ChromiumBrowserProvider));
        services.ShouldContain(d => d.ServiceType == typeof(BookArtifactService));
        services.ShouldContain(d => d.ServiceType == typeof(IFileWatchAware));
        services.ShouldContain(d => d.ServiceType == typeof(IContentEmitter));
        services.ShouldContain(d => d.ServiceType == typeof(BookComposer));
        services.ShouldContain(d => d.ServiceType == typeof(AssetInliner));
    }

    [Fact]
    public void Captures_configured_books_on_the_options_singleton()
    {
        var services = new ServiceCollection();
        services.AddPenningtonBook(o =>
        {
            o.Books.Add(new BookDefinition("Guides", "/how-to/"));
            o.AdditionalChromiumArgs = ["--no-sandbox"];
        });

        var options = services.BuildServiceProvider().GetRequiredService<BookOptions>();

        options.Books.ShouldHaveSingleItem().Title.ShouldBe("Guides");
        options.AdditionalChromiumArgs.ShouldBe(["--no-sandbox"]);
    }
}
