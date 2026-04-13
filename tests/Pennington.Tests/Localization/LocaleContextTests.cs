using Pennington.Infrastructure;
using Pennington.Localization;

namespace Pennington.Tests.Localization;

public class LocaleContextTests
{
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