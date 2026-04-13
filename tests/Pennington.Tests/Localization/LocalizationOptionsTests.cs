using Pennington.Infrastructure;
using Pennington.Localization;

namespace Pennington.Tests.Localization;

public class LocalizationOptionsTests
{
    private static LocalizationOptions CreateMultiLocale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("Français"));
        options.AddLocale("de", new LocaleInfo("Deutsch"));
        return options;
    }

    private static LocalizationOptions CreateSingleLocale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        return options;
    }

    // --- GetLocaleFromUrl ---

    [Fact]
    public void GetLocaleFromUrl_SingleLocale_ReturnsDefault()
    {
        var options = CreateSingleLocale();
        options.GetLocaleFromUrl("/fr/about").ShouldBe("en");
    }

    [Fact]
    public void GetLocaleFromUrl_DetectsNonDefaultLocale()
    {
        var options = CreateMultiLocale();
        options.GetLocaleFromUrl("/fr/about").ShouldBe("fr");
    }

    [Fact]
    public void GetLocaleFromUrl_DetectsLocaleWithoutTrailingPath()
    {
        var options = CreateMultiLocale();
        options.GetLocaleFromUrl("/fr").ShouldBe("fr");
    }

    [Fact]
    public void GetLocaleFromUrl_UnknownSegment_ReturnsDefault()
    {
        var options = CreateMultiLocale();
        options.GetLocaleFromUrl("/es/about").ShouldBe("en");
    }

    [Fact]
    public void GetLocaleFromUrl_DefaultLocalePrefix_ReturnsDefault()
    {
        var options = CreateMultiLocale();
        options.GetLocaleFromUrl("/en/about").ShouldBe("en");
    }

    [Fact]
    public void GetLocaleFromUrl_RootPath_ReturnsDefault()
    {
        var options = CreateMultiLocale();
        options.GetLocaleFromUrl("/").ShouldBe("en");
    }

    [Fact]
    public void GetLocaleFromUrl_EmptyPath_ReturnsDefault()
    {
        var options = CreateMultiLocale();
        options.GetLocaleFromUrl("").ShouldBe("en");
    }

    // --- StripLocalePrefix ---

    [Fact]
    public void StripLocalePrefix_DefaultLocale_Unchanged()
    {
        var options = CreateMultiLocale();
        options.StripLocalePrefix("/about/", "en").ShouldBe("/about/");
    }

    [Fact]
    public void StripLocalePrefix_NonDefaultLocale_RemovesPrefix()
    {
        var options = CreateMultiLocale();
        options.StripLocalePrefix("/fr/about/", "fr").ShouldBe("/about/");
    }

    [Fact]
    public void StripLocalePrefix_LocaleOnly_ReturnsRoot()
    {
        var options = CreateMultiLocale();
        options.StripLocalePrefix("/fr", "fr").ShouldBe("/");
    }

    [Fact]
    public void StripLocalePrefix_NoMatch_ReturnsOriginal()
    {
        var options = CreateMultiLocale();
        options.StripLocalePrefix("/about/", "fr").ShouldBe("/about/");
    }

    // --- BuildLocaleUrl ---

    [Fact]
    public void BuildLocaleUrl_DefaultLocale_NoPrefix()
    {
        var options = CreateMultiLocale();
        options.BuildLocaleUrl("about", "en").ShouldBe("/about/");
    }

    [Fact]
    public void BuildLocaleUrl_DefaultLocale_EmptyPath()
    {
        var options = CreateMultiLocale();
        options.BuildLocaleUrl("", "en").ShouldBe("/");
    }

    [Fact]
    public void BuildLocaleUrl_NonDefaultLocale_AddsPrefix()
    {
        var options = CreateMultiLocale();
        options.BuildLocaleUrl("about", "fr").ShouldBe("/fr/about/");
    }

    [Fact]
    public void BuildLocaleUrl_NonDefaultLocale_EmptyPath()
    {
        var options = CreateMultiLocale();
        options.BuildLocaleUrl("", "fr").ShouldBe("/fr/");
    }

    [Fact]
    public void BuildLocaleUrl_StripsLeadingAndTrailingSlashes()
    {
        var options = CreateMultiLocale();
        options.BuildLocaleUrl("/about/", "fr").ShouldBe("/fr/about/");
    }
}