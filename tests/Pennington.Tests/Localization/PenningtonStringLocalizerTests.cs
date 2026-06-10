using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Infrastructure;
using Pennington.Localization;

namespace Pennington.Tests.Localization;

public class PenningtonStringLocalizerTests
{
    private static LocalizationOptions CreateOptions()
    {
        var locOptions = new LocalizationOptions { DefaultLocale = "en" };
        locOptions.AddLocale("en", new LocaleInfo("English"));
        locOptions.AddLocale("fr", new LocaleInfo("Français"));
        locOptions.AddLocale("gen-z", new LocaleInfo("Gen Z", HtmlLang: "en-genz"));
        return locOptions;
    }

    private static TranslationOptions CreateTranslations()
    {
        var translations = new TranslationOptions();
        translations.Add("en", "greeting", "Hello");
        translations.Add("en", "farewell", "Goodbye");
        translations.Add("fr", "greeting", "Bonjour");
        translations.Add("gen-z", "greeting", "Yo");
        return translations;
    }

    /// <summary>
    /// Builds a localizer whose request is pinned to <paramref name="locale"/>, mirroring what
    /// <see cref="LocaleDetectionMiddleware"/> writes into the scoped <see cref="LocaleContext"/>.
    /// </summary>
    private static PenningtonStringLocalizer LocalizerForLocale(
        string locale, LocalizationOptions locOptions, TranslationOptions translations)
    {
        var info = locOptions.Locales.TryGetValue(locale, out var li) ? li : new LocaleInfo(locale);
        var context = new LocaleContext(locOptions)
        {
            Locale = locale,
            Info = info,
            IsDefaultLocale = string.Equals(locale, locOptions.DefaultLocale, StringComparison.OrdinalIgnoreCase),
        };

        var services = new ServiceCollection();
        services.AddSingleton(context);
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        return new PenningtonStringLocalizer(translations, locOptions, new StubHttpContextAccessor(httpContext));
    }

    [Fact]
    public void Returns_Translation_For_Request_Locale()
    {
        var localizer = LocalizerForLocale("fr", CreateOptions(), CreateTranslations());
        localizer["greeting"].Value.ShouldBe("Bonjour");
    }

    [Fact]
    public void Falls_Back_To_Default_Locale()
    {
        var localizer = LocalizerForLocale("fr", CreateOptions(), CreateTranslations());
        // "farewell" only exists in "en"
        localizer["farewell"].Value.ShouldBe("Goodbye");
    }

    [Fact]
    public void Falls_Back_To_Key_When_Not_Found()
    {
        var localizer = LocalizerForLocale("fr", CreateOptions(), CreateTranslations());
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

        var localizer = LocalizerForLocale("en", locOptions, translations);
        localizer["welcome", "Alice"].Value.ShouldBe("Welcome, Alice!");
    }

    [Fact]
    public void GetAllStrings_Returns_Current_Locale_Entries()
    {
        var localizer = LocalizerForLocale("fr", CreateOptions(), CreateTranslations());
        var strings = localizer.GetAllStrings(includeParentCultures: false).ToList();
        strings.ShouldContain(s => s.Name == "greeting" && s.Value == "Bonjour");
        strings.ShouldNotContain(s => s.Name == "farewell");
    }

    [Fact]
    public void GetAllStrings_IncludeParent_Merges_Default()
    {
        var localizer = LocalizerForLocale("fr", CreateOptions(), CreateTranslations());
        var strings = localizer.GetAllStrings(includeParentCultures: true).ToList();
        strings.ShouldContain(s => s.Name == "greeting" && s.Value == "Bonjour");
        strings.ShouldContain(s => s.Name == "farewell" && s.Value == "Goodbye");
    }

    [Fact]
    public void Uses_LocaleContext_Not_CurrentUICulture()
    {
        // Regression for F141: the locale comes from the middleware-populated LocaleContext,
        // not from a fragile CultureInfo.CurrentUICulture round-trip. Even a conflicting
        // ambient culture must not change the resolved translation.
        var localizer = LocalizerForLocale("fr", CreateOptions(), CreateTranslations());

        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("en");
        try
        {
            localizer["greeting"].Value.ShouldBe("Bonjour");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void Falls_Back_To_Default_Locale_Outside_A_Request()
    {
        // No HttpContext (e.g. design-time / non-request usage) => default locale.
        var localizer = new PenningtonStringLocalizer(CreateTranslations(), CreateOptions());
        localizer["greeting"].Value.ShouldBe("Hello");
    }

    [Fact]
    public async Task Consumes_The_Locale_LocaleDetectionMiddleware_Detected()
    {
        // End-to-end: the middleware detects "/fr/about" and populates the scoped LocaleContext;
        // the localizer reaches that same context through IHttpContextAccessor — no duplicate detection.
        var locOptions = CreateOptions();
        var translations = CreateTranslations();

        var services = new ServiceCollection();
        services.AddSingleton(locOptions);
        services.AddScoped<LocaleContext>();
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        httpContext.Request.Path = new PathString("/fr/about");

        var middleware = new LocaleDetectionMiddleware(_ => Task.CompletedTask, locOptions);
        await middleware.Invoke(httpContext);

        var localizer = new PenningtonStringLocalizer(
            translations, locOptions, new StubHttpContextAccessor(httpContext));

        localizer["greeting"].Value.ShouldBe("Bonjour");
    }

    private sealed class StubHttpContextAccessor(HttpContext? context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }
}
