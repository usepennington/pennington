namespace Pennington.Islands;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

/// <summary>
/// Renders Blazor components to HTML strings using static HtmlRenderer.
/// Registered as Scoped — shared across a request, disposed at scope end.
/// </summary>
public sealed class ComponentRenderer(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : IAsyncDisposable
{
    private readonly HtmlRenderer _renderer = new(serviceProvider, loggerFactory);

    public Task<string> RenderComponentAsync<TComponent>(
        IDictionary<string, object?>? parameters = null)
        where TComponent : IComponent
    {
        return _renderer.Dispatcher.InvokeAsync(async () =>
        {
            var pv = parameters is not null
                ? ParameterView.FromDictionary(parameters)
                : ParameterView.Empty;

            var output = await _renderer.RenderComponentAsync<TComponent>(pv);
            return output.ToHtmlString();
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _renderer.DisposeAsync();
    }
}
