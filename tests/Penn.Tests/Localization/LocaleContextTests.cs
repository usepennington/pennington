using Penn.Infrastructure;
using Penn.Localization;

namespace Penn.Tests.Localization;

public class LocaleContextTests
{
    [Fact]
    public void Defaults_To_DefaultLocale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("Français"));

        var ctx = new LocaleContext(options);

        ctx.Locale.ShouldBe("en");
        ctx.IsDefaultLocale.ShouldBeTrue();
        ctx.ContentPath.ShouldBe("/");
        ctx.Info.DisplayName.ShouldBe("English");
    }

    [Fact]
    public void HtmlLang_Uses_LocaleInfo_HtmlLang_When_Set()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("gen-z", new LocaleInfo("Gen Z", HtmlLang: "en-genz"));

        var ctx = new LocaleContext(options)
        {
            Locale = "gen-z",
            Info = options.Locales["gen-z"],
        };

        ctx.HtmlLang.ShouldBe("en-genz");
    }

    [Fact]
    public void HtmlLang_Falls_Back_To_Locale_Code()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("fr", new LocaleInfo("Français"));

        var ctx = new LocaleContext(options)
        {
            Locale = "fr",
            Info = options.Locales["fr"],
        };

        ctx.HtmlLang.ShouldBe("fr");
    }

    [Fact]
    public void Direction_Reads_From_LocaleInfo()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("ar", new LocaleInfo("Arabic", Direction: "rtl"));

        var ctx = new LocaleContext(options)
        {
            Locale = "ar",
            Info = options.Locales["ar"],
        };

        ctx.Direction.ShouldBe("rtl");
    }

    [Fact]
    public void Url_Builds_Locale_Aware_Url()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("Français"));

        var ctx = new LocaleContext(options) { Locale = "fr", Info = options.Locales["fr"], IsDefaultLocale = false };

        ctx.Url("/about").ShouldBe("/fr/about/");
    }

    [Fact]
    public void Url_DefaultLocale_NoPrefix()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("Français"));

        var ctx = new LocaleContext(options);

        ctx.Url("/about").ShouldBe("/about/");
    }
}
