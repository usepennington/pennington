namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.TestHost;

/// <summary>
/// Default <see cref="IInProcessHttpDispatcher"/>. Inspects the registered
/// <see cref="IServer"/> and returns an in-memory client when it's a
/// <see cref="TestServer"/>, or a socket-bound client pointing at Kestrel's
/// listening address otherwise.
/// <para>
/// The handler chain is built once and shared by every client this dispatcher hands out
/// — clients are cheap wrappers created with <c>disposeHandler: false</c>, so a caller's
/// <c>using</c> releases the wrapper without tearing down the connection pool. This
/// matters on the Kestrel path: <see cref="Pipeline.SiteProjection"/> issues one fetch per
/// page, and a per-fetch handler cannot pool, so every fetch opened a fresh loopback socket
/// that then sat in TIME_WAIT. A corpus larger than the ~16k Windows ephemeral port range
/// exhausted it partway through and the remaining fetches failed with
/// <c>SocketError.AddressAlreadyInUse</c> — silently dropping those pages from the search
/// index and llms.txt, since <c>RenderOneAsync</c> treats a failed fetch as a per-page error.
/// </para>
/// </summary>
public sealed class HttpDispatcher : IInProcessHttpDispatcher, IDisposable
{
    private readonly IServer _server;
    private readonly BuildHtmlCache _cache;

    private readonly Lock _handlerLock = new();
    private HttpMessageHandler? _handler;
    private bool _disposed;

    /// <summary>Initializes the dispatcher with the host's registered <see cref="IServer"/> and the shared render cache.</summary>
    public HttpDispatcher(IServer server, BuildHtmlCache cache)
    {
        _server = server;
        _cache = cache;
    }

    /// <inheritdoc/>
    public HttpClient CreateClient()
    {
        var (handler, baseAddress) = GetOrCreateHandler();

        // disposeHandler: false — the chain outlives every client so connections stay pooled.
        return new HttpClient(handler, disposeHandler: false) { BaseAddress = baseAddress };
    }

    /// <summary>Disposes the shared handler chain and its pooled connections.</summary>
    public void Dispose()
    {
        lock (_handlerLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _handler?.Dispose();
            _handler = null;
        }
    }

    private (HttpMessageHandler Handler, Uri BaseAddress) GetOrCreateHandler()
    {
        lock (_handlerLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // The base address is recomputed per call: it is cheap, and on the Kestrel path
            // it is only known once the server has bound. Only the handler is cached.
            var baseAddress = ResolveBaseAddress();
            _handler ??= BuildHandler();
            return (_handler, baseAddress);
        }
    }

    private Uri ResolveBaseAddress()
    {
        if (_server is TestServer)
        {
            // TestServer.CreateClient() returns BaseAddress = http://localhost/.
            // Path-relative URLs ("/foo/bar") resolve against that and are dispatched
            // to the same RequestDelegate Kestrel would have invoked.
            return new Uri("http://localhost/");
        }

        var addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses;
        if (addresses is null || addresses.Count == 0)
        {
            throw new SelfFetchUnavailableException(
                "HttpDispatcher requires either a started TestServer or a listening Kestrel host. " +
                "IServerAddressesFeature has no addresses — is the app started yet?");
        }

        // Prefer http:// to avoid dev-cert trust issues when self-fetching from Kestrel.
        var baseAddress = addresses.FirstOrDefault(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            ?? addresses.First();
        return new Uri(baseAddress);
    }

    private HttpMessageHandler BuildHandler()
    {
        HttpMessageHandler innerHandler;
        if (_server is TestServer testServer)
        {
            try
            {
                innerHandler = testServer.CreateHandler();
            }
            catch (InvalidOperationException ex)
            {
                // TestServer.Application is null until the host's IServer has started. A
                // self-fetch issued before that — e.g. a startup hosted service racing the
                // server start — is an infrastructure failure, not a per-page content error.
                // Surface it as such so callers retry once the host is up instead of baking
                // an empty corpus. Nothing is cached, so the retry rebuilds.
                throw new SelfFetchUnavailableException(
                    "The in-process TestServer has not started yet; a self-fetch was issued before " +
                    "the host's server was ready.",
                    ex);
            }
        }
        else
        {
            // Bounded pool: the projection renders pages with Parallel.ForEachAsync at the
            // default degree of parallelism, so a handful of pooled connections serves the
            // whole corpus. Left unbounded, a burst opens far more sockets than it reuses.
            innerHandler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                MaxConnectionsPerServer = Environment.ProcessorCount * 2,
            };
        }

        // Wrap with the cache so repeat self-fetches replay one render.
        return new CachingHttpHandler(_cache) { InnerHandler = innerHandler };
    }
}