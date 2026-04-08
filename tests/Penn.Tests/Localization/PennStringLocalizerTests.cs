using System.Globalization;
using Penn.Infrastructure;
using Penn.Localization;

namespace Penn.Tests.Localization;

public class PennStringLocalizerTests
{
    private static (PennStringLocalizer Localizer, TranslationOptions Translations) CreateLocalizer()
    {
        var locOptions = new LocalizationOptions { DefaultLocale = "en" };
        locOptions.AddLocale("en", new LocaleInfo("English"));
        locOptions.AddLocale("fr", new LocaleInfo("Français"));
        locOptions.AddLocale("gen-z", new LocaleInfo("Gen Z", HtmlLang: "en-genz"));

        var translations = new TranslationOptions();
        translations.Add("en", "greeting", "Hello");
        translations.Add("en", "farewell", "Goodbye");
        translations.Add("fr", "greeting", "Bonjour");
        translations.Add("gen-z", "greeting", "Yo");

        return (new PennStringLocalizer(translations, locOptions), translations);
    }

    [Fact]
    public void Returns_Translation_For_Current_Culture()
    {
        var (localizer, _) = CreateLocalizer();

        using var _ = SetCulture("fr");
        localizer["greeting"].Value.ShouldBe("Bonjour");
    }

    [Fact]
    public void Falls_Back_To_Default_Locale()
    {
        var (localizer, _) = CreateLocalizer();

        using var _ = SetCulture("fr");
        // "farewell" only exists in "en"
        localizer["farewell"].Value.ShouldBe("Goodbye");
    }

    [Fact]
    public void Falls_Back_To_Key_When_Not_Found()
    {
        var (localizer, _) = CreateLocalizer();

        using var _ = SetCulture("fr");
        var result = localizer["unknown.key"];
        result.Value.ShouldBe("unknown.key");
        result.ResourceNotFound.ShouldBeTrue();
    }

    [Fact]
    public void Supports_Format_Arguments()
    {
        var locOptions = new LocalizationOptions { DefaultLocale = "en" };
        locOptions.AddLocale("en", new LocaleInfo("English"));

        var translations = new TranslationOptions();
        translations.Add("en", "welcome", "Welcome, {0}!");

        var localizer = new PennStringLocalizer(translations, locOptions);

        using var _ = SetCulture("en");
        localizer["welcome", "Alice"].Value.ShouldBe("Welcome, Alice!");
    }

    [Fact]
    public void GetAllStrings_Returns_Current_Locale_Entries()
    {
        var (localizer, _) = CreateLocalizer();

        using var _ = SetCulture("fr");
        var strings = localizer.GetAllStrings(includeParentCultures: false).ToList();
        strings.ShouldContain(s => s.Name == "greeting" && s.Value == "Bonjour");
        strings.ShouldNotContain(s => s.Name == "farewell");
    }

    [Fact]
    public void GetAllStrings_IncludeParent_Merges_Default()
    {
        var (localizer, _) = CreateLocalizer();

        using var _ = SetCulture("fr");
        var strings = localizer.GetAllStrings(includeParentCultures: true).ToList();
        strings.ShouldContain(s => s.Name == "greeting" && s.Value == "Bonjour");
        strings.ShouldContain(s => s.Name == "farewell" && s.Value == "Goodbye");
    }

    private static IDisposable SetCulture(string name)
    {
        var prev = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(name);
        return new CultureRestorer(prev);
    }

    private sealed class CultureRestorer(CultureInfo previous) : IDisposable
    {
        public void Dispose() => CultureInfo.CurrentUICulture = previous;
    }
}
