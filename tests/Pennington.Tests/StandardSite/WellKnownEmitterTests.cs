using System.Text;
using Pennington.StandardSite;

namespace Pennington.Tests.StandardSite;

/// <summary>Unit tests for <see cref="WellKnownEmitter"/> output and fail-safe behavior.</summary>
public class WellKnownEmitterTests
{
    private static async Task<Dictionary<string, string>> Emit(StandardSiteOptions options)
    {
        var files = await new WellKnownEmitter(options).GetContentToCreateAsync();
        var result = new Dictionary<string, string>();
        foreach (var file in files)
        {
            result[file.OutputPath.Value] = Encoding.UTF8.GetString(await file.ContentGenerator());
        }

        return result;
    }

    [Fact]
    public async Task EmitsPublicationWellKnown_WithBareAtUri()
    {
        var files = await Emit(new StandardSiteOptions { Did = "did:plc:abc", PublicationRkey = "pub1" });

        files[".well-known/site.standard.publication"]
            .ShouldBe("at://did:plc:abc/site.standard.publication/pub1");
    }

    [Fact]
    public async Task OmitsAtprotoDid_ByDefault()
    {
        var files = await Emit(new StandardSiteOptions { Did = "did:plc:abc", PublicationRkey = "pub1" });

        files.ShouldNotContainKey(".well-known/atproto-did");
    }

    [Fact]
    public async Task EmitsAtprotoDid_WhenEnabled()
    {
        var files = await Emit(new StandardSiteOptions
        {
            Did = "did:plc:abc",
            PublicationRkey = "pub1",
            EmitAtprotoDid = true,
        });

        files[".well-known/atproto-did"].ShouldBe("did:plc:abc");
    }

    [Fact]
    public async Task AppendsPublicationPath_ForSubPathPublications()
    {
        var files = await Emit(new StandardSiteOptions
        {
            Did = "did:plc:abc",
            PublicationRkey = "pub1",
            PublicationPath = "/blog",
        });

        files.ShouldContainKey(".well-known/site.standard.publication/blog");
    }

    [Fact]
    public async Task EmitsNothing_WhenConfigIsBlank()
    {
        var files = await Emit(new StandardSiteOptions { Did = "", PublicationRkey = "" });

        files.ShouldBeEmpty();
    }
}
