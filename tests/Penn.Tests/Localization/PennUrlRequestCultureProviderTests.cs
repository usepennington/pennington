using Pennington.Infrastructure;
using Pennington.Localization;

namespace Pennington.Tests.Localization;

public class PenningtonUrlRequestCultureProviderTests
{
    [Fact]
    public void Maps_Standard_Locale_Code_Directly()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("Français"));

        var provider = new PenningtonUrlRequestCultureProvider(options);

        provider.MapToCultureName("fr").ShouldBe("fr");
    }

    [Fact]
    public void Maps_Via_HtmlLang()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("sv", new LocaleInfo("Bork Bork", HtmlLang: "sv"));

        var provider = new PenningtonUrlRequestCultureProvider(options);

        provider.MapToCultureName("sv").ShouldBe("sv");
    }

    [Fact]
    public void Maps_Custom_Locale_Via_HtmlLang()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("gen-z", new LocaleInfo("Gen Z", HtmlLang: "en-genz"));

        var provider = new PenningtonUrlRequestCultureProvider(options);

        // .NET accepts "en-genz" as a valid culture name
        provider.MapToCultureName("gen-z").ShouldBe("en-genz");
    }

    [Fact]
    public void Falls_Back_To_Default_For_Unknown_Locale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("pirate", new LocaleInfo("Pirate")); // No HtmlLang, "pirate" is not a culture

        var provider = new PenningtonUrlRequestCultureProvider(options);

        provider.MapToCultureName("pirate").ShouldBe("en");
    }

    [Fact]
    public void Maps_Default_Locale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));

        var provider = new PenningtonUrlRequestCultureProvider(options);

        provider.MapToCultureName("en").ShouldBe("en");
    }
}
