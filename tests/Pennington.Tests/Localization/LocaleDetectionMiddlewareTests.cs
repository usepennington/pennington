using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Infrastructure;
using Pennington.Localization;

namespace Pennington.Tests.Localization;

public class LocaleDetectionMiddlewareTests
{
    private static LocalizationOptions CreateLocalization()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("French"));
        return options;
    }

    private static async Task<HttpContext> InvokeAsync(string path)
    {
        var localization = CreateLocalization();

        var services = new ServiceCollection();
        services.AddSingleton(localization);
        services.AddScoped<LocaleContext>();

        var ctx = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };
        ctx.Request.Path = new PathString(path);

        var middleware = new LocaleDetectionMiddleware(_ => Task.CompletedTask, localization);
        await middleware.Invoke(ctx);
        return ctx;
    }

    [Fact]
    public async Task BareLocaleRoot_MovesFullLocaleSegmentToPathBase()
    {
        // Regression: "/fr" must strip to PathBase "/fr" + Path "/", not "/f".
        var ctx = await InvokeAsync("/fr");

        ctx.Request.PathBase.Value.ShouldBe("/fr");
        ctx.Request.Path.Value.ShouldBe("/");
    }

    [Fact]
    public async Task LocalePrefixedPath_StripsLocaleToPathBase()
    {
        var ctx = await InvokeAsync("/fr/about");

        ctx.Request.PathBase.Value.ShouldBe("/fr");
        ctx.Request.Path.Value.ShouldBe("/about");
    }

    [Fact]
    public async Task DefaultLocale_LeavesPathUnchanged()
    {
        var ctx = await InvokeAsync("/about");

        ctx.Request.PathBase.Value.ShouldBeNullOrEmpty();
        ctx.Request.Path.Value.ShouldBe("/about");
    }
}
