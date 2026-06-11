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
        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = _options.AdditionalChromiumArgs,
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
