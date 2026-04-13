using Pennington.Infrastructure;
using Pennington.Localization;

namespace Pennington.Tests.Localization;

public class GetAlternateLanguagesTests
{
    private static LocalizationOptions CreateOptions()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("Français", HtmlLang: "fr"));
        options.AddLocale("gen-z", new LocaleInfo("Gen Z", HtmlLang: "en-genz"));
        return options;
    }

    [Fact]
    public void Returns_Empty_For_SingleLocale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));

        options.GetAlternateLanguages("/about").ShouldBeEmpty();
    }

    [Fact]
    public void Returns_All_Locales()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/about");

        alternates.Count.ShouldBe(3);
        alternates.Select(a => a.Locale).ShouldBe(["en", "fr", "gen-z"]);
    }

    [Fact]
    public void Marks_Current_Locale_For_DefaultLocale_Url()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/about");

        alternates.First(a => a.Locale == "en").IsCurrentLocale.ShouldBeTrue();
        alternates.First(a => a.Locale == "fr").IsCurrentLocale.ShouldBeFalse();
    }

    [Fact]
    public void Marks_Current_Locale_For_NonDefaultLocale_Url()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/fr/about");

        alternates.First(a => a.Locale == "fr").IsCurrentLocale.ShouldBeTrue();
        alternates.First(a => a.Locale == "en").IsCurrentLocale.ShouldBeFalse();
    }

    [Fact]
    public void Builds_Correct_Urls()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/about");

        alternates.First(a => a.Locale == "en").Url.ShouldBe("/about/");
        alternates.First(a => a.Locale == "fr").Url.ShouldBe("/fr/about/");
        alternates.First(a => a.Locale == "gen-z").Url.ShouldBe("/gen-z/about/");
    }

    [Fact]
    public void Uses_HtmlLang_From_LocaleInfo()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/about");

        alternates.First(a => a.Locale == "en").HtmlLang.ShouldBe("en");
        alternates.First(a => a.Locale == "fr").HtmlLang.ShouldBe("fr");
        alternates.First(a => a.Locale == "gen-z").HtmlLang.ShouldBe("en-genz");
    }

    [Fact]
    public void Handles_Root_Url()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/");

        alternates.First(a => a.Locale == "en").Url.ShouldBe("/");
        alternates.First(a => a.Locale == "fr").Url.ShouldBe("/fr/");
    }

    [Fact]
    public void Normalizes_Index_To_Root()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/index");

        alternates.First(a => a.Locale == "en").Url.ShouldBe("/");
    }

    [Fact]
    public void Returns_Locale_Roots_For_NotFoundGenerator_Sentinel()
    {
        // The 404 generator sentinel path is not a real content page. GetAlternateLanguages
        // must treat it as the landing page so each locale's switcher link points at
        // that locale's root rather than phantom /{locale}/__pennington-404-generator/ URLs.
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/__pennington-404-generator");

        alternates.First(a => a.Locale == "en").Url.ShouldBe("/");
        alternates.First(a => a.Locale == "fr").Url.ShouldBe("/fr/");
        alternates.First(a => a.Locale == "gen-z").Url.ShouldBe("/gen-z/");
        alternates.Select(a => a.Url).ShouldNotContain(u => u.Contains("__pennington-404-generator"));
    }

    [Fact]
    public void Returns_Locale_Roots_For_NotFoundGenerator_Sentinel_With_TrailingSlash()
    {
        var options = CreateOptions();
        var alternates = options.GetAlternateLanguages("/__pennington-404-generator/");

        alternates.First(a => a.Locale == "en").Url.ShouldBe("/");
        alternates.First(a => a.Locale == "fr").Url.ShouldBe("/fr/");
        alternates.First(a => a.Locale == "gen-z").Url.ShouldBe("/gen-z/");
        alternates.Select(a => a.Url).ShouldNotContain(u => u.Contains("__pennington-404-generator"));
    }
}