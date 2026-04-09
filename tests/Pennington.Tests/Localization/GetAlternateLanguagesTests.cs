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
}
