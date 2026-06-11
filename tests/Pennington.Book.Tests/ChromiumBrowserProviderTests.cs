namespace Pennington.Book.Tests;

using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Book;
using Pennington.Book.Rendering;

public sealed class ChromiumBrowserProviderTests
{
    [Fact]
    public async Task Renders_html_to_pdf_bytes_when_chromium_is_available()
    {
        var provider = new ChromiumBrowserProvider(new BookOptions(), NullLogger<ChromiumBrowserProvider>.Instance);

        byte[] pdf;
        try
        {
            // The HTML signals readiness immediately, the same contract paged.js fulfils in a real book.
            pdf = await provider.RenderPdfAsync(
                "<!DOCTYPE html><html><head><script>window.__pagedDone = true;</script></head>"
                + "<body><h1>Smoke</h1><p>PDF rendering works.</p></body></html>",
                TestContext.Current.CancellationToken);
        }
        catch (Exception ex)
        {
            // No Chromium (offline CI, no system Chrome, download blocked): skip, don't fail.
            Assert.Skip($"Chromium unavailable for PDF smoke test: {ex.Message}");
            return;
        }
        finally
        {
            await provider.DisposeAsync();
        }

        pdf.Length.ShouldBeGreaterThan(4);
        Encoding.ASCII.GetString(pdf, 0, 4).ShouldBe("%PDF");
    }
}
