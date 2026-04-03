using Penn.Localization;
using Penn.Routing;

namespace Penn.Tests.Localization;

public class LocalizationTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void LocaleInfo_CreateWithAllFields()
    {
        var info = new LocaleInfo(
            DisplayName: "English (US)",
            Direction: "ltr",
            HtmlLang: "en-US");

        info.DisplayName.ShouldBe("English (US)");
        info.Direction.ShouldBe("ltr");
        info.HtmlLang.ShouldBe("en-US");
    }

    [Fact]
    public void LocaleInfo_DefaultDirection_IsLtr()
    {
        var info = new LocaleInfo(DisplayName: "Deutsch");

        info.Direction.ShouldBe("ltr");
        info.HtmlLang.ShouldBeNull();
    }

    [Fact]
    public void LocaleInfo_RtlLocale()
    {
        var info = new LocaleInfo(
            DisplayName: "العربية",
            Direction: "rtl",
            HtmlLang: "ar");

        info.DisplayName.ShouldBe("العربية");
        info.Direction.ShouldBe("rtl");
        info.HtmlLang.ShouldBe("ar");
    }

    [Fact]
    public void AlternateLanguagePage_CreateAndVerifyProperties()
    {
        var route = MakeRoute("/fr/docs/intro");

        var alternate = new AlternateLanguagePage(
            Locale: "fr",
            DisplayName: "Fran\u00e7ais",
            Route: route);

        alternate.Locale.ShouldBe("fr");
        alternate.DisplayName.ShouldBe("Fran\u00e7ais");
        alternate.Route.CanonicalPath.Value.ShouldBe("/fr/docs/intro");
    }
}
