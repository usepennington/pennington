using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

/// <summary>
/// End-to-end tests that run <see cref="LocaleLinkRewritingProcessor"/> and
/// <see cref="BaseUrlRewritingProcessor"/> in their registered Order and assert
/// that href rewriting is correct for a multi-locale site deployed to a base
/// URL. Regression coverage for Phase 2 of issue-plan.md — the two processors
/// must run in the order (locale=20, baseUrl=30) so the base URL is the
/// outermost transport layer.
/// </summary>
public class LocaleAndBaseUrlPipelineTests
{
    private const string BaseUrl = "/preview/";

    private static LocalizationOptions CreateLocalization()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("kl", new LocaleInfo("Klingon", HtmlLang: "tlh"));
        options.AddLocale("pl", new LocaleInfo("Pig Latin"));
        return options;
    }

    private static OutputOptions CreateOutputOptions() => new()
    {
        OutputDirectory = new FilePath("output"),
        BaseUrl = new UrlPath(BaseUrl),
    };

    private static DefaultHttpContext CreateContext(string locale)
    {
        var context = new DefaultHttpContext();
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html";
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost", 5000);
        context.Items["Pennington.Locale"] = locale;
        return context;
    }

    /// <summary>
    /// Runs the two processors in ascending Order, mirroring what
    /// ResponseProcessingMiddleware does at runtime.
    /// </summary>
    private static async Task<string> RunChainAsync(string inputHtml, string locale)
    {
        var localization = CreateLocalization();
        var outputOptions = CreateOutputOptions();
        IResponseProcessor[] processors =
        [
            new LocaleLinkRewritingProcessor(localization),
            new BaseUrlRewritingProcessor(outputOptions),
        ];

        var context = CreateContext(locale);
        var body = inputHtml;
        foreach (var processor in processors.OrderBy(p => p.Order))
        {
            if (processor.ShouldProcess(context))
            {
                body = await processor.ProcessAsync(body, context);
            }
        }

        return body;
    }

    [Fact]
    public async Task DefaultLocale_AppliesOnlyBaseUrl()
    {
        var input = """<html><body><a href="/about/">About</a></body></html>""";

        var result = await RunChainAsync(input, locale: "en");

        result.ShouldContain("href=\"/preview/about/\"");
        result.ShouldNotContain("/en/about/");
    }

    [Fact]
    public async Task NonDefaultLocale_LogicalLink_AppliesLocaleThenBase()
    {
        // Regression A — the primary bug shape from issues.md:
        // A root-relative nav link like /about/ must end up as /preview/kl/about/,
        // NOT /kl/preview/about/.
        var input = """<html><body><a href="/about/">About</a></body></html>""";

        var result = await RunChainAsync(input, locale: "kl");

        result.ShouldContain("href=\"/preview/kl/about/\"");
        result.ShouldNotContain("/kl/preview/");
    }

    [Fact]
    public async Task NonDefaultLocale_AlreadyLocalePrefixedLink_PassesThroughThenBase()
    {
        // Regression B — NavigationBuilder emits /kl/about/ for locale-subtree pages.
        // The locale processor must detect the existing /kl/ prefix and pass through;
        // the base URL processor then produces /preview/kl/about/, NOT /preview/kl/kl/about/.
        var input = """<html><body><a href="/kl/about/">About</a></body></html>""";

        var result = await RunChainAsync(input, locale: "kl");

        result.ShouldContain("href=\"/preview/kl/about/\"");
        result.ShouldNotContain("/preview/kl/kl/");
        result.ShouldNotContain("/kl/preview/");
    }

    [Fact]
    public async Task NonDefaultLocale_CrossLocaleLink_PassesThroughLocaleThenBase()
    {
        // A link from a Klingon page to a Pig Latin page must keep /pl/ and
        // receive only the base URL prefix (no Klingon insertion).
        var input = """<html><body><a href="/pl/faq/">Pig Latin FAQ</a></body></html>""";

        var result = await RunChainAsync(input, locale: "kl");

        result.ShouldContain("href=\"/preview/pl/faq/\"");
        result.ShouldNotContain("/kl/pl/");
        result.ShouldNotContain("/preview/kl/pl/");
    }

    [Fact]
    public async Task NonDefaultLocale_AbsoluteSameSiteUrl_LocaleRewrites_BaseSkips()
    {
        // Absolute URLs (http://localhost:5000/about/) get locale-rewritten by the
        // locale processor (it recognizes the origin and rewrites the path). The
        // base URL processor skips them because they don't start with '/'.
        var input = """<html><body><a href="http://localhost:5000/about/">About</a></body></html>""";

        var result = await RunChainAsync(input, locale: "kl");

        result.ShouldContain("href=\"http://localhost:5000/kl/about/\"");
    }

    [Fact]
    public async Task ExternalUrl_IsUnchanged()
    {
        var input = """<html><body><a href="https://github.com/example">GitHub</a></body></html>""";

        var result = await RunChainAsync(input, locale: "kl");

        result.ShouldContain("href=\"https://github.com/example\"");
    }

    [Fact]
    public async Task LanguageSwitcherLink_WithDataLocale_SkipsLocaleRewrite_AppliesBase()
    {
        // Language-switcher links are marked with data-locale so the locale
        // processor leaves them alone (they intentionally point at a specific
        // locale). Base URL rewriting still applies.
        var input = """<html><body><a href="/pl/" data-locale="pl">Pig Latin</a></body></html>""";

        var result = await RunChainAsync(input, locale: "kl");

        result.ShouldContain("href=\"/preview/pl/\"");
        result.ShouldContain("data-locale=\"pl\"");
        result.ShouldNotContain("/kl/pl/");
    }

    [Fact]
    public async Task FragmentAndMailto_AreUnchanged()
    {
        var input = """<html><body><a href="#top">Top</a><a href="mailto:x@y.com">Mail</a></body></html>""";

        var result = await RunChainAsync(input, locale: "kl");

        result.ShouldContain("href=\"#top\"");
        result.ShouldContain("href=\"mailto:x@y.com\"");
    }

    [Fact]
    public void LocaleLinkRewritingProcessor_RunsBefore_BaseUrlRewritingProcessor()
    {
        var localeProcessor = new LocaleLinkRewritingProcessor(CreateLocalization());
        var baseProcessor = new BaseUrlRewritingProcessor(CreateOutputOptions());

        localeProcessor.Order.ShouldBeLessThan(baseProcessor.Order);
    }

    [Fact]
    public void BuiltInResponseProcessors_FormExpectedOrderChain()
    {
        // Locks in the 10/20/30/40/50 sequence for the five built-in processors.
        // Adding a new processor? Slot it at 15, 25, 35, 45, or outside this range —
        // don't disturb the sequence.
        var xref = new XrefResolvingProcessor(new XrefResolvingService(
            new ServiceCollection().BuildServiceProvider()));
        var locale = new LocaleLinkRewritingProcessor(CreateLocalization());
        var baseUrl = new BaseUrlRewritingProcessor(CreateOutputOptions());
        var liveReload = new LiveReloadScriptProcessor();
        var diagOverlay = new DiagnosticOverlayProcessor();

        IResponseProcessor[] processors = [xref, locale, baseUrl, liveReload, diagOverlay];
        var ordered = processors.OrderBy(p => p.Order).ToArray();

        ordered[0].ShouldBeOfType<XrefResolvingProcessor>().Order.ShouldBe(10);
        ordered[1].ShouldBeOfType<LocaleLinkRewritingProcessor>().Order.ShouldBe(20);
        ordered[2].ShouldBeOfType<BaseUrlRewritingProcessor>().Order.ShouldBe(30);
        ordered[3].ShouldBeOfType<LiveReloadScriptProcessor>().Order.ShouldBe(40);
        ordered[4].ShouldBeOfType<DiagnosticOverlayProcessor>().Order.ShouldBe(50);
    }
}
