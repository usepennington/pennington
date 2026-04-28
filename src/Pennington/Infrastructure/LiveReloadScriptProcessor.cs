namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Injects a live reload script into HTML responses during development.
/// Skipped during static build so the output HTML is clean.
/// </summary>
public sealed class LiveReloadScriptProcessor : IResponseProcessor
{
    private readonly bool _isDevMode = !PenningtonBuildMode.IsBuildMode();

    // Runs after the HTML rewriting pipeline (10) but before the
    // diagnostic overlay (30).
    /// <inheritdoc/>
    public int Order => 20;

    /// <inheritdoc/>
    public bool ShouldProcess(HttpContext context)
    {
        if (!_isDevMode) return false;

        var contentType = context.Response.ContentType ?? "";
        return context.Response.StatusCode is >= 200 and < 300
            && contentType.Contains("text/html");
    }

    /// <inheritdoc/>
    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        const string script = """
            <script>
            (function(){
                var p=(location.protocol==='https:'?'wss://':'ws://')+location.host+'/__pennington/reload';
                var isClosing=false;
                window.addEventListener('beforeunload',function(){
                    isClosing=true;
                    setTimeout(function(){isClosing=false;},2500);
                });
                function connect(){
                    if(isClosing)return;
                    var ws=new WebSocket(p);
                    ws.onopen=function(){
                        if(connect.was){setTimeout(function(){location.reload();},150);return;}
                        connect.was=true;
                    };
                    ws.onmessage=function(){setTimeout(function(){location.reload();},150);};
                    ws.onclose=function(){if(!isClosing)setTimeout(connect,500);};
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
