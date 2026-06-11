namespace Pennington.Book.Tests;

using System.IO.Abstractions;
using Microsoft.Extensions.FileProviders;
using Pennington.Book.Composition;
using Pennington.Routing;
using Testably.Abstractions.Testing;

public sealed class AssetInlinerTests
{
    private static AssetInliner Create(IFileSystem fileSystem)
        => new(fileSystem, new NullFileProvider(), new CanonicalBaseUrl(new UrlPath("https://example.com")));

    [Fact]
    public void Resolves_a_content_map_image_to_a_data_uri()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory(@"C:\site\assets");
        var bytes = new byte[] { 1, 2, 3, 4 };
        fs.File.WriteAllBytes(@"C:\site\assets\logo.png", bytes);

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["assets/logo.png"] = @"C:\site\assets\logo.png",
        };

        var result = Create(fs).Resolve("/assets/logo.png", map);

        result.ShouldBe($"data:image/png;base64,{Convert.ToBase64String(bytes)}");
    }

    [Fact]
    public void Leaves_external_images_untouched()
    {
        var inliner = Create(new MockFileSystem());
        var map = new Dictionary<string, string>();

        inliner.Resolve("https://cdn.example.com/a.png", map).ShouldBe("https://cdn.example.com/a.png");
        inliner.Resolve("//cdn.example.com/b.png", map).ShouldBe("//cdn.example.com/b.png");
    }

    [Fact]
    public void Absolutizes_unresolved_internal_images()
    {
        var inliner = Create(new MockFileSystem());

        var result = inliner.Resolve("/img/missing.png", new Dictionary<string, string>());

        result.ShouldBe("https://example.com/img/missing.png");
    }

    [Fact]
    public void Strips_query_and_fragment_before_lookup()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory(@"C:\site\img");
        fs.File.WriteAllBytes(@"C:\site\img\d.gif", new byte[] { 9 });
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["img/d.gif"] = @"C:\site\img\d.gif",
        };

        Create(fs).Resolve("/img/d.gif?v=2", map).ShouldStartWith("data:image/gif;base64,");
    }
}
