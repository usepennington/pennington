using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Beck;
using Pennington.Head;

namespace Pennington.Tests.Beck;

/// <summary>
/// Tests for <see cref="BeckZoomHeadContributor"/> — the <see cref="BeckOptions.Zoom"/> gate,
/// the contributed lightbox assets, and its registration by
/// <see cref="BeckServiceExtensions.AddPenningtonBeck"/>.
/// </summary>
public class BeckZoomHeadContributorTests
{
    private static HeadContext CreateContext() =>
        new() { HttpContext = new DefaultHttpContext(), FullPath = "/docs/page" };

    [Fact]
    public void ShouldContribute_ZoomOn_IsTrue()
    {
        var contributor = new BeckZoomHeadContributor(new BeckOptions());

        contributor.ShouldContribute(CreateContext()).ShouldBeTrue();
    }

    [Fact]
    public void ShouldContribute_ZoomOff_IsFalse()
    {
        var contributor = new BeckZoomHeadContributor(new BeckOptions { Zoom = false });

        contributor.ShouldContribute(CreateContext()).ShouldBeFalse();
    }

    [Fact]
    public async Task ContributeAsync_AddsOneKeyedRawTagWithLightboxAssets()
    {
        var contributor = new BeckZoomHeadContributor(new BeckOptions());
        var head = new HeadBuilder();

        await contributor.ContributeAsync(CreateContext(), head);

        var entries = head.Build();
        var entry = entries.ShouldHaveSingleItem();
        entry.Key.ShouldBe(new HeadTagKey("beck:zoom"));
        var raw = entry.Tag.Value.ShouldBeOfType<RawTag>();
        raw.Html.ShouldContain("beck-lightbox");
        raw.Html.ShouldContain(".beck-zoom");
    }

    [Fact]
    public void AddPenningtonBeck_RegistersTheZoomContributor()
    {
        var services = new ServiceCollection();

        services.AddPenningtonBeck();

        services.ShouldContain(d =>
            d.ServiceType == typeof(IHeadContributor)
            && d.ImplementationType == typeof(BeckZoomHeadContributor));
    }
}
