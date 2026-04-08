using Penn.Infrastructure;

namespace Penn.Tests.Infrastructure;

public class FontPreloadTests
{
    [Fact]
    public void DefaultType_IsWoff2()
    {
        var preload = new FontPreload("fonts/test.woff2");
        preload.Type.ShouldBe("font/woff2");
    }

    [Fact]
    public void CustomType_IsPreserved()
    {
        var preload = new FontPreload("fonts/test.woff", "font/woff");
        preload.Type.ShouldBe("font/woff");
    }

    [Fact]
    public void Href_IsStored()
    {
        var preload = new FontPreload("fonts/lexend.woff2");
        preload.Href.ShouldBe("fonts/lexend.woff2");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new FontPreload("fonts/test.woff2");
        var b = new FontPreload("fonts/test.woff2");
        a.ShouldBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentHref_AreNotEqual()
    {
        var a = new FontPreload("fonts/a.woff2");
        var b = new FontPreload("fonts/b.woff2");
        a.ShouldNotBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentType_AreNotEqual()
    {
        var a = new FontPreload("fonts/test.woff2", "font/woff2");
        var b = new FontPreload("fonts/test.woff2", "font/woff");
        a.ShouldNotBe(b);
    }
}
