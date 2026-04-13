using Pennington.Infrastructure;
using Pennington.Localization;

namespace Pennington.Tests.Localization;

public class PenningtonUrlRequestCultureProviderTests
{
    [Fact]
    public void Falls_Back_To_Default_For_Unknown_Locale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("pirate", new LocaleInfo("Pirate")); // No HtmlLang, "pirate" is not a culture

        var provider = new PenningtonUrlRequestCultureProvider(options);

        provider.MapToCultureName("pirate").ShouldBe("en");
    }

}