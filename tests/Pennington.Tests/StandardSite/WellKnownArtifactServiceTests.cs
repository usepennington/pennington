using System.Text;
using Pennington.StandardSite;

namespace Pennington.Tests.StandardSite;

/// <summary>Unit tests for <see cref="WellKnownArtifactService"/> output and fail-safe behavior.</summary>
public class WellKnownArtifactServiceTests
{
    private static async Task<Dictionary<string, string>> Emit(StandardSiteOptions options)
    {
        var service = new WellKnownArtifactService(options);
        var result = new Dictionary<string, string>();
        await foreach (var item in service.DiscoverAsync())
        {
            var content = await service.ResolveAsync(
                item.Route.CanonicalPath.Value.TrimStart('/'),
                TestContext.Current.CancellationToken);
            content.ShouldNotBeNull();
            result[item.Route.OutputFile.Value] = Encoding.UTF8.GetString(content.Bytes);
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

    [Fact]
    public void ClaimsNothing_WhenConfigIsBlank()
    {
        var service = new WellKnownArtifactService(new StandardSiteOptions { Did = "", PublicationRkey = "" });

        service.Claims.ShouldBeEmpty();
    }

    [Fact]
    public async Task Resolve_UnknownPath_Declines()
    {
        var service = new WellKnownArtifactService(new StandardSiteOptions
        {
            Did = "did:plc:abc",
            PublicationRkey = "pub1",
            EmitAtprotoDid = true,
        });

        // Prefix-adjacent but not exact — must decline so the request falls through.
        var content = await service.ResolveAsync(".well-known/atproto-did-x", TestContext.Current.CancellationToken);

        content.ShouldBeNull();
    }
}
