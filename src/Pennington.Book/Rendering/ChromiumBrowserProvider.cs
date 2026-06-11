namespace Pennington.Book.Rendering;

using Microsoft.Extensions.Logging;
using Pennington.Infrastructure;
using PuppeteerSharp;

/// <summary>
/// Owns the process-lifetime Chromium instance used to render composed book HTML to PDF.
/// Registered as a DI singleton — the documented "connection pool" exception to the
/// transient/file-watched default — because launching Chromium is expensive and the browser
/// is safe to reuse across renders. PDF rendering is serialized through a semaphore so a
/// single browser never juggles concurrent paginations.
/// </summary>
public sealed class ChromiumBrowserProvider : IAsyncDisposable
{
    private readonly BookOptions _options;
    private readonly ILogger<ChromiumBrowserProvider> _logger;
    private readonly AsyncLazy<IBrowser> _browser;
    private readonly SemaphoreSlim _renderGate = new(1, 1);
    private IBrowser? _live;

    /// <summary>Creates the provider; Chromium is launched lazily on the first render.</summary>
    public ChromiumBrowserProvider(BookOptions options, ILogger<ChromiumBrowserProvider> logger)
    {
        _options = options;
        _logger = logger;
        _browser = new AsyncLazy<IBrowser>(LaunchAsync);
    }

    /// <summary>
    /// Renders <paramref name="html"/> to PDF bytes. The HTML must signal completion by setting
    /// <c>window.__pagedDone = true</c> (the paged.js <c>after</c> hook the composer wires up);
    /// rendering waits up to two minutes for that flag before producing the PDF.
    /// </summary>
    public async Task<byte[]> RenderPdfAsync(string html, CancellationToken cancellationToken = default)
    {
        var browser = await _browser;
        await _renderGate.WaitAsync(cancellationToken);
        try
        {
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);
            await page.WaitForFunctionAsync(
                "() => window.__pagedDone === true",
                new WaitForFunctionOptions { Timeout = 120_000 });

            return await page.PdfDataAsync(new PdfOptions
            {
                PreferCSSPageSize = true,
                PrintBackground = true,
                Outline = true,
                Tagged = true,
            });
        }
        finally
        {
            _renderGate.Release();
        }
    }

    private async Task<IBrowser> LaunchAsync()
    {
        var isLinux = OperatingSystem.IsLinux();
        var isContinuousIntegration = IsContinuousIntegration();
        if (isLinux && isContinuousIntegration)
        {
            _logger.LogInformation("Linux CI detected; launching Chromium with --no-sandbox (book content is the site's own trusted HTML).");
        }

        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = ResolveChromiumArgs(_options.AdditionalChromiumArgs, isLinux, isContinuousIntegration),
        };

        if (!string.IsNullOrWhiteSpace(_options.ChromiumExecutablePath))
        {
            launchOptions.ExecutablePath = _options.ChromiumExecutablePath;
            _logger.LogInformation("Launching Chromium from {Path}", _options.ChromiumExecutablePath);
        }
        else
        {
            _logger.LogInformation("Resolving Chromium for PDF rendering (downloading ~150 MB on first run only)...");
            var installed = await new BrowserFetcher().DownloadAsync();
            launchOptions.ExecutablePath = installed.GetExecutablePath();
            _logger.LogInformation("Using Chromium build {BuildId}", installed.BuildId);
        }

        var browser = await Puppeteer.LaunchAsync(launchOptions);
        _live = browser;
        return browser;
    }

    /// <summary>
    /// Resolves Chromium launch arguments, appending the sandbox-disabling flags on Linux CI runners
    /// where AppArmor blocks unprivileged user namespaces (Ubuntu 23.10+, GitHub Actions). Chromium's
    /// sandbox guards against malicious web pages; book content is always the site's own trusted HTML,
    /// so dropping it in CI is safe. Pure and parameterized for testing.
    /// </summary>
    internal static string[] ResolveChromiumArgs(IReadOnlyList<string> additionalArgs, bool isLinux, bool isContinuousIntegration)
    {
        var args = new List<string>(additionalArgs);

        if (isLinux && isContinuousIntegration)
        {
            AddIfMissing(args, "--no-sandbox");
            AddIfMissing(args, "--disable-setuid-sandbox");
        }

        return [.. args];

        static void AddIfMissing(List<string> list, string flag)
        {
            if (!list.Contains(flag))
            {
                list.Add(flag);
            }
        }
    }

    /// <summary>True when a CI marker env var (<c>CI</c> or <c>GITHUB_ACTIONS</c>) is set to a truthy value.</summary>
    private static bool IsContinuousIntegration()
    {
        static bool Truthy(string? value) =>
            !string.IsNullOrEmpty(value)
            && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
            && value != "0";

        return Truthy(Environment.GetEnvironmentVariable("CI"))
            || Truthy(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));
    }

    /// <summary>Closes the browser process if one was launched.</summary>
    public async ValueTask DisposeAsync()
    {
        _renderGate.Dispose();
        if (_live is { } browser)
        {
            try
            {
                await browser.CloseAsync();
            }
            catch
            {
                // Best-effort shutdown; the process may already be gone.
            }

            await browser.DisposeAsync();
        }
    }
}
