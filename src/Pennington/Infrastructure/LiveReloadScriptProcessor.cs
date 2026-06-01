namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Injects a live reload script into HTML responses during development.
/// Skipped during static build so the output HTML is clean.
/// </summary>
internal sealed class LiveReloadScriptProcessor : IResponseProcessor
{
    private readonly bool _isDevMode = !PenningtonBuildMode.IsHeadlessOneShot;

    // Per-process fingerprint. Lets the SPA engine in a still-open browser tab
    // from a previous dev session detect that it's now talking to a different
    // host (port reuse across `dotnet run` invocations) and drop its prefetch
    // cache instead of committing stale content.
    private static readonly string HostFingerprint = Guid.NewGuid().ToString("N");

    // Runs after the HTML rewriting pipeline (10) but before the
    // diagnostic overlay (30).
    /// <inheritdoc/>
    public int Order => 20;

    /// <inheritdoc/>
    public bool ShouldProcess(HttpContext context)
    {
        if (!_isDevMode)
        {
            return false;
        }

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

        var bodyIdx = responseBody.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyIdx >= 0)
        {
            responseBody = responseBody.Insert(bodyIdx, script);
        }

        var hostMeta = $"<meta name=\"x-pennington-host\" content=\"{HostFingerprint}\">";
        var headIdx = responseBody.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headIdx >= 0)
        {
            responseBody = responseBody.Insert(headIdx, hostMeta);
        }

        return Task.FromResult(responseBody);
    }
}