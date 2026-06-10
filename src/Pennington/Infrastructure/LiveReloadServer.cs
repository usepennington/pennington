namespace Pennington.Infrastructure;

using System.Collections.Concurrent;
using System.Net.WebSockets;
using Cli;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages WebSocket connections for live reload during development.
/// When watched files change, debounces rapid notifications and sends
/// a single reload message to all connected browsers.
/// </summary>
public sealed class LiveReloadServer : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new();
    private readonly ILogger<LiveReloadServer>? _logger;
    private readonly CancellationToken _shutdownToken;
    private Timer? _debounceTimer;
    private bool _disposed;

    /// <summary>Initializes the server and subscribes to watched-file change notifications.</summary>
    public LiveReloadServer(IFileWatcher fileWatcher, IHostApplicationLifetime lifetime, ILogger<LiveReloadServer>? logger = null)
    {
        _logger = logger;
        _shutdownToken = lifetime.ApplicationStopping;
        fileWatcher.SubscribeToChanges(DebouncedNotify);
    }

    private void DebouncedNotify()
    {
        // Reset the timer on every notification — NotifyClients fires only
        // after 300ms of quiet, coalescing rapid saves into one reload.
        _debounceTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _debounceTimer ??= new Timer(_ => NotifyClients());
        _debounceTimer.Change(300, Timeout.Infinite);
    }

    private void NotifyClients()
    {
        var message = "reload"u8.ToArray();

        foreach (var (id, socket) in _clients)
        {
            if (socket.State == WebSocketState.Open)
            {
                _ = socket.SendAsync(message, WebSocketMessageType.Text, endOfMessage: true, _shutdownToken);
            }
            else
            {
                _clients.TryRemove(id, out _);
            }
        }

        _logger?.LogDebug("Sent reload to {Count} connected client(s)", _clients.Count);
    }

    internal async Task HandleAsync(WebSocket socket, CancellationToken requestAborted)
    {
        var id = Guid.NewGuid().ToString("N");
        _clients[id] = socket;
        _logger?.LogDebug("Live reload client connected ({Id})", id);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_shutdownToken, requestAborted);
        try
        {
            var buffer = new byte[64];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, linked.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _clients.TryRemove(id, out _);
            if (socket.State == WebSocketState.Open && !_shutdownToken.IsCancellationRequested)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }
    }

    /// <summary>Disposes the debounce timer and closes any open client sockets.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_debounceTimer is { } timer)
        {
            _debounceTimer = null;
            try { await timer.DisposeAsync(); }
            catch (Exception ex) { _logger?.LogWarning(ex, "Error disposing live reload debounce timer"); }
        }

        // Give each socket a short window to close cleanly; never let a wedged
        // client stall teardown.
        using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        foreach (var (id, socket) in _clients)
        {
            _clients.TryRemove(id, out _);
            if (socket.State != WebSocketState.Open)
            {
                continue;
            }

            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, closeCts.Token);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Error closing live reload socket {Id}", id);
            }
        }
    }
}

/// <summary>Extensions that wire the live reload WebSocket endpoint into the request pipeline.</summary>
public static class LiveReloadExtensions
{
    internal const string ReloadPath = "/__pennington/reload";

    /// <summary>
    /// Adds live reload WebSocket support for development.
    /// Skipped during static build (see <see cref="PenningtonCli"/>).
    /// </summary>
    public static WebApplication UseLiveReload(this WebApplication app)
    {
        if (PenningtonCli.Current.IsHeadlessOneShot)
        {
            return app;
        }

        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == ReloadPath && context.WebSockets.IsWebSocketRequest)
            {
                var server = context.RequestServices.GetRequiredService<LiveReloadServer>();
                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                await server.HandleAsync(socket, context.RequestAborted);
            }
            else
            {
                await next(context);
            }
        });

        return app;
    }
}