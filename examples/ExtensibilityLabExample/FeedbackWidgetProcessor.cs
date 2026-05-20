namespace ExtensibilityLabExample;

using System.Text;
using Microsoft.AspNetCore.Http;
using Pennington.Infrastructure;

/// <summary>
/// Implements <see cref="IResponseProcessor"/>. Injects a
/// "Was this helpful?" footer before the closing <c>&lt;/body&gt;</c>
/// tag of every rendered HTML page.
/// <para>
/// Runs at <see cref="Order"/> 500 — after the xref/locale/base-URL HTML
/// rewriting processor (<c>HtmlResponseRewritingProcessor</c>) so the
/// injected HTML is not subject to any further pipeline passes in this
/// app, and well before the live-reload and diagnostic-overlay
/// processors at 1000+.
/// </para>
/// <para>
/// <see cref="ShouldProcess"/> gates on content type: text/html only,
/// and only for 2xx responses. Static assets and API JSON skip through.
/// </para>
/// <para>
/// Backs how-to 2.3.40 <c>/how-to/extensibility/response-processor</c>.
/// </para>
/// </summary>
public sealed class FeedbackWidgetProcessor : IResponseProcessor
{
    private const string WidgetHtml = """
        <aside class="feedback-widget" data-extensibility-lab="feedback-widget">
          <p><strong>Was this helpful?</strong>
            <button type="button" data-feedback="yes">Yes</button>
            <button type="button" data-feedback="no">No</button>
          </p>
        </aside>
        """;

    public int Order => 500;

    public bool ShouldProcess(HttpContext context)
    {
        if (context.Response.StatusCode is < 200 or >= 300)
        {
            return false;
        }

        var contentType = context.Response.ContentType;
        return contentType is not null
               && contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        if (string.IsNullOrEmpty(responseBody))
        {
            return Task.FromResult(responseBody);
        }

        var closeBodyIndex = responseBody.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (closeBodyIndex < 0)
        {
            // No </body> — append at end. Still visible, still verifiable.
            return Task.FromResult(responseBody + WidgetHtml);
        }

        var sb = new StringBuilder(responseBody.Length + WidgetHtml.Length);
        sb.Append(responseBody, 0, closeBodyIndex);
        sb.Append(WidgetHtml);
        sb.Append(responseBody, closeBodyIndex, responseBody.Length - closeBodyIndex);
        return Task.FromResult(sb.ToString());
    }
}