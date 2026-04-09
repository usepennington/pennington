namespace Pennington.Infrastructure;

using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages WebSocket connections for live reload during development.
/// When watched files change, sends a reload message to all connected browsers.
/// </summary>
public sealed class LiveReloadServer
{
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new();
    private readonly ILogger<LiveReloadServer>? _logger;

    public LiveReloadServer(IFileWatcher fileWatcher, ILogger<LiveReloadServer>? logger = null)
    {
        _logger = logger;
        fileWatcher.SubscribeToChanges(NotifyClients);
    }

    private void NotifyClients()
    {
        var message = "reload"u8.ToArray();

        foreach (var (id, socket) in _clients)
        {
            if (socket.State == WebSocketState.Open)
            {
                _ = socket.SendAsync(message, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
            }
            else
            {
                _clients.TryRemove(id, out _);
            }
        }

        _logger?.LogDebug("Sent reload to {Count} connected client(s)", _clients.Count);
    }

    internal async Task HandleAsync(WebSocket socket)
    {
        var id = Guid.NewGuid().ToString("N");
        _clients[id] = socket;
        _logger?.LogDebug("Live reload client connected ({Id})", id);

        try
        {
            var buffer = new byte[64];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }
        finally
        {
            _clients.TryRemove(id, out _);
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }
    }
}

public static class LiveReloadExtensions
{
    internal const string ReloadPath = "/__pennington/reload";

    /// <summary>
    /// Adds live reload WebSocket support for development. Only active when DOTNET_WATCH is set.
    /// </summary>
    public static WebApplication UsePenningtonLiveReload(this WebApplication app)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH")))
            return app;

        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == ReloadPath && context.WebSockets.IsWebSocketRequest)
            {
                var server = context.RequestServices.GetRequiredService<LiveReloadServer>();
                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                await server.HandleAsync(socket);
            }
            else
            {
                await next(context);
            }
        });

        return app;
    }
}
