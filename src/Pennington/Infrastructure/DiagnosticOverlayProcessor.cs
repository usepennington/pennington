namespace Pennington.Infrastructure;

using System.Text.Json;
using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Injects a diagnostic overlay widget into HTML responses during development.
/// Shows a floating badge with warning/error counts that expands to show details.
/// Updates on SPA navigations via the spa:diagnostics custom event.
/// Only active during dev-serve (disabled during static build).
/// </summary>
public sealed class DiagnosticOverlayProcessor : IResponseProcessor
{
    private readonly bool _isDevMode = !PenningtonBuildMode.IsBuildMode();

    // Runs after the HTML rewriting pipeline (10).
    /// <inheritdoc/>
    public int Order => 30;

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
        var diagnosticContext = context.RequestServices.GetService<DiagnosticContext>();
        var diagnostics = diagnosticContext?.Diagnostics ?? [];

        var json = JsonSerializer.Serialize(diagnostics.Select(d => new
        {
            severity = d.Severity.ToString().ToLowerInvariant(),
            message = d.Message,
            source = d.Source
        }));

        var overlay = $$"""
            <div id="penn-diag-root" style="position:fixed;bottom:20px;right:20px;z-index:99999;font-family:ui-sans-serif,system-ui,-apple-system,sans-serif;font-size:13px;line-height:1.5"></div>
            <script>
            (function(){
                var root=document.getElementById('penn-diag-root');
                var current={{json}};
                var expanded=false;

                function sev(list){
                    if(list.some(function(d){return d.severity==='error'}))return'error';
                    if(list.some(function(d){return d.severity==='warning'}))return'warning';
                    return'info';
                }

                var theme={
                    error:  {dot:'#ef4444',text:'#fca5a5'},
                    warning:{dot:'#f59e0b',text:'#fde68a'},
                    info:   {dot:'#3b82f6',text:'#93c5fd'}
                };

                function render(){
                    if(!current.length){root.innerHTML='';return;}
                    var s=sev(current);
                    var t=theme[s];
                    var errors=current.filter(function(d){return d.severity==='error'}).length;
                    var warnings=current.filter(function(d){return d.severity==='warning'}).length;
                    var parts=[];
                    if(errors)parts.push(errors+' error'+(errors>1?'s':''));
                    if(warnings)parts.push(warnings+' warning'+(warnings>1?'s':''));
                    if(!parts.length)parts.push(current.length+' info');

                    var html='<div id="penn-diag-badge" style="cursor:pointer;user-select:none;'
                        +'display:inline-flex;align-items:center;gap:8px;'
                        +'padding:8px 16px;border-radius:12px;'
                        +'background:rgba(15,23,42,.65);backdrop-filter:blur(12px);-webkit-backdrop-filter:blur(12px);'
                        +'border:1px solid rgba(255,255,255,.08);'
                        +'color:#e2e8f0;font-weight:500;font-size:13px;'
                        +'box-shadow:0 2px 8px rgba(0,0,0,.15),0 0 0 1px rgba(0,0,0,.1);'
                        +'transition:all .15s ease">'
                        +'<span style="width:7px;height:7px;border-radius:50%;background:'+t.dot
                        +';flex-shrink:0;box-shadow:0 0 6px '+t.dot+'"></span>'
                        +'<span style="color:'+t.text+'">'+parts.join(', ')+'</span>'
                        +'<span style="font-size:10px;opacity:.4;margin-left:2px">'+(expanded?'\u25BC':'\u25B2')+'</span>'
                        +'</div>';

                    if(expanded){
                        html+='<style>'
                            +'#penn-diag-panel::-webkit-scrollbar{width:6px}'
                            +'#penn-diag-panel::-webkit-scrollbar-track{background:transparent}'
                            +'#penn-diag-panel::-webkit-scrollbar-thumb{background:rgba(255,255,255,.1);border-radius:3px}'
                            +'#penn-diag-panel::-webkit-scrollbar-thumb:hover{background:rgba(255,255,255,.18)}'
                            +'#penn-diag-panel{scrollbar-width:thin;scrollbar-color:rgba(255,255,255,.1) transparent}'
                            +'</style>';
                        html+='<div id="penn-diag-panel" style="position:absolute;bottom:100%;right:0;margin-bottom:8px;'
                            +'background:rgba(15,23,42,.92);backdrop-filter:blur(16px);-webkit-backdrop-filter:blur(16px);'
                            +'color:#e2e8f0;border-radius:12px;'
                            +'box-shadow:0 8px 32px rgba(0,0,0,.3),0 0 0 1px rgba(255,255,255,.06);'
                            +'max-height:360px;overflow-y:auto;width:420px;padding:4px 0">'
                            +'<div style="padding:10px 16px 8px;font-size:11px;font-weight:600;'
                            +'text-transform:uppercase;letter-spacing:.05em;color:#64748b;'
                            +'border-bottom:1px solid rgba(255,255,255,.06)">Diagnostics</div>';
                        current.forEach(function(d,i){
                            var c=d.severity==='error'?'#f87171':d.severity==='warning'?'#fbbf24':'#93c5fd';
                            var pillBg=d.severity==='error'?'rgba(239,68,68,.12)':d.severity==='warning'?'rgba(245,158,11,.12)':'rgba(59,130,246,.12)';
                            html+='<div style="padding:10px 16px;'
                                +(i<current.length-1?'border-bottom:1px solid rgba(255,255,255,.04);':'')
                                +'">'
                                +'<div style="display:flex;align-items:center;gap:8px;margin-bottom:3px">'
                                +'<span style="display:inline-block;font-size:10px;font-weight:700;'
                                +'text-transform:uppercase;letter-spacing:.05em;color:'+c+';'
                                +'padding:2px 7px;border-radius:4px;background:'+pillBg+'">'
                                +d.severity+'</span>'
                                +(d.source?'<span style="font-size:11px;color:#64748b">'+d.source+'</span>':'')
                                +'</div>'
                                +'<div style="font-size:12.5px;color:#cbd5e1;word-break:break-word">'
                                +d.message.replace(/</g,'&lt;')+'</div>'
                                +'</div>';
                        });
                        html+='</div>';
                    }
                    root.innerHTML=html;

                    var badge=document.getElementById('penn-diag-badge');
                    if(badge){
                        badge.onmouseenter=function(){this.style.background='rgba(15,23,42,.75)';this.style.borderColor='rgba(255,255,255,.12)';this.style.boxShadow='0 4px 16px rgba(0,0,0,.2),0 0 0 1px rgba(0,0,0,.15)'};
                        badge.onmouseleave=function(){this.style.background='rgba(15,23,42,.65)';this.style.borderColor='rgba(255,255,255,.08)';this.style.boxShadow='0 2px 8px rgba(0,0,0,.15),0 0 0 1px rgba(0,0,0,.1)'};
                        badge.onclick=function(){expanded=!expanded;render()};
                    }
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