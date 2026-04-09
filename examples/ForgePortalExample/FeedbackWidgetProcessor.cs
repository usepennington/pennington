namespace ForgePortalExample;

using Penn.Infrastructure;
using Microsoft.AspNetCore.Http;

public class FeedbackWidgetProcessor : IResponseProcessor
{
    public int Order => 500;

    public bool ShouldProcess(HttpContext context)
    {
        var contentType = context.Response.ContentType ?? "";
        if (!contentType.Contains("text/html")) return false;
        if (context.Request.Path.Value?.Contains("search-index") == true) return false;
        return true;
    }

    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        const string widget = """
            <div id="feedback-widget" style="position:fixed;bottom:1rem;right:1rem;z-index:50;">
                <button onclick="alert('Feedback submitted!')"
                        style="padding:0.5rem 1rem;background:#3b82f6;color:white;border:none;border-radius:0.375rem;cursor:pointer;">
                    Feedback
                </button>
            </div>
            """;

        var insertIndex = responseBody.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (insertIndex >= 0)
        {
            return Task.FromResult(responseBody.Insert(insertIndex, widget));
        }

        return Task.FromResult(responseBody);
    }
}
