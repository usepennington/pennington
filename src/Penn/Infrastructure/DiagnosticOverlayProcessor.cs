namespace Penn.Infrastructure;

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Penn.Diagnostics;

/// <summary>
/// Injects a diagnostic overlay widget into HTML responses during development.
/// Shows a floating badge with warning/error counts that expands to show details.
/// Updates on SPA navigations via the spa:diagnostics custom event.
/// Only active when DOTNET_WATCH environment variable is set.
/// </summary>
public sealed class DiagnosticOverlayProcessor : IResponseProcessor
{
    private static readonly bool IsDevMode = !string.IsNullOrEmpty(
        Environment.GetEnvironmentVariable("DOTNET_WATCH"));

    public int Order => 10000;

    public bool ShouldProcess(HttpContext context)
    {
        if (!IsDevMode) return false;
        var contentType = context.Response.ContentType ?? "";
        return context.Response.StatusCode is >= 200 and < 300
            && contentType.Contains("text/html");
    }

    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var diagnosticContext = context.RequestServices.GetService<DiagnosticContext>();
        var diagnostics = diagnosticContext?.Diagnostics ?? [];

        var json = JsonSerializer.Serialize(diagnostics.Select(d => new
        {
            severity = d.Severity.ToString().ToLowerInvariant(),
            message = d.Message,
            source = d.Source
        }));

        var overlay = $$"""
            <div id="penn-diag-root" style="position:fixed;bottom:12px;right:12px;z-index:99999;font-family:system-ui,-apple-system,sans-serif;font-size:13px"></div>
            <script>
            (function(){
                var root=document.getElementById('penn-diag-root');
                var current={{json}};
                var expanded=false;

                function severity(list){
                    if(list.some(function(d){return d.severity==='error'}))return'error';
                    if(list.some(function(d){return d.severity==='warning'}))return'warning';
                    return'info';
                }

                function render(){
                    if(!current.length){root.innerHTML='';return;}
                    var sev=severity(current);
                    var colors={error:'#dc2626',warning:'#d97706',info:'#2563eb'};
                    var bg=colors[sev]||colors.info;
                    var errors=current.filter(function(d){return d.severity==='error'}).length;
                    var warnings=current.filter(function(d){return d.severity==='warning'}).length;
                    var parts=[];
                    if(errors)parts.push(errors+' error'+(errors>1?'s':''));
                    if(warnings)parts.push(warnings+' warning'+(warnings>1?'s':''));
                    if(!parts.length)parts.push(current.length+' info');

                    var html='<div style="cursor:pointer;background:'+bg+';color:#fff;padding:4px 10px;border-radius:6px;'
                        +'box-shadow:0 2px 8px rgba(0,0,0,.2);user-select:none" id="penn-diag-badge">'
                        +parts.join(', ')+'</div>';

                    if(expanded){
                        html+='<div style="position:absolute;bottom:100%;right:0;margin-bottom:6px;background:#1e1e1e;'
                            +'color:#d4d4d4;border-radius:8px;box-shadow:0 4px 16px rgba(0,0,0,.3);max-height:300px;'
                            +'overflow-y:auto;width:380px;padding:8px 0">';
                        current.forEach(function(d){
                            var c=d.severity==='error'?'#f87171':d.severity==='warning'?'#fbbf24':'#93c5fd';
                            html+='<div style="padding:4px 12px;border-bottom:1px solid #333">'
                                +'<span style="color:'+c+';font-weight:600;text-transform:uppercase;font-size:11px">'
                                +d.severity+'</span> '
                                +'<span>'+d.message.replace(/</g,'&lt;')+'</span>'
                                +(d.source?'<span style="color:#888;margin-left:6px;font-size:11px">'+d.source+'</span>':'')
                                +'</div>';
                        });
                        html+='</div>';
                    }
                    root.innerHTML=html;

                    var badge=document.getElementById('penn-diag-badge');
                    if(badge)badge.onclick=function(){expanded=!expanded;render();};
                }

                render();
                document.addEventListener('spa:diagnostics',function(e){
                    current=e.detail||[];
                    expanded=false;
                    render();
                });
            })();
            </script>
            """;

        var idx = responseBody.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            responseBody = responseBody.Insert(idx, overlay);

        return Task.FromResult(responseBody);
    }
}
