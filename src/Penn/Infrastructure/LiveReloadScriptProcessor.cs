namespace Penn.Infrastructure;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Injects a live reload script into HTML responses during development.
/// Only active when DOTNET_WATCH environment variable is set.
/// </summary>
public sealed class LiveReloadScriptProcessor : IResponseProcessor
{
    private static readonly bool IsDevMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"));

    // Run after other processors
    public int Order => 1000;

    public bool ShouldProcess(HttpContext context)
    {
        if (!IsDevMode) return false;

        var contentType = context.Response.ContentType ?? "";
        return context.Response.StatusCode is >= 200 and < 300
            && contentType.Contains("text/html");
    }

    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        const string script = """
            <script>
            (function(){
                var p=(location.protocol==='https:'?'wss://':'ws://')+location.host+'/__penn/reload';
                function connect(){
                    var ws=new WebSocket(p);
                    ws.onmessage=function(){location.reload();};
                    ws.onclose=function(){setTimeout(connect,1000);};
                }
                connect();
            })();
            </script>
            """;

        var idx = responseBody.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            responseBody = responseBody.Insert(idx, script);
        }

        return Task.FromResult(responseBody);
    }
}
