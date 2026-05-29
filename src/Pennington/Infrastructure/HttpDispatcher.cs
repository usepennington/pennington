namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.TestHost;

/// <summary>
/// Default <see cref="IInProcessHttpDispatcher"/>. Inspects the registered
/// <see cref="IServer"/> and returns an in-memory client when it's a
/// <see cref="TestServer"/>, or a socket-bound client pointing at Kestrel's
/// listening address otherwise.
/// </summary>
public sealed class HttpDispatcher : IInProcessHttpDispatcher
{
    private readonly IServer _server;
    private readonly BuildHtmlCache _cache;

    /// <summary>Initializes the dispatcher with the host's registered <see cref="IServer"/> and the shared render cache.</summary>
    public HttpDispatcher(IServer server, BuildHtmlCache cache)
    {
        _server = server;
        _cache = cache;
    }

    /// <inheritdoc/>
    public HttpClient CreateClient()
    {
        if (_server is TestServer testServer)
        {
            // TestServer.CreateClient() returns BaseAddress = http://localhost/.
            // Path-relative URLs ("/foo/bar") resolve against that and are dispatched
            // to the same RequestDelegate Kestrel would have invoked. Wrap its handler
            // with the cache so repeat self-fetches replay one render.
            HttpMessageHandler innerHandler;
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
                // an empty corpus.
                throw new SelfFetchUnavailableException(
                    "The in-process TestServer has not started yet; a self-fetch was issued before " +
                    "the host's server was ready.",
                    ex);
            }

            var testHandler = new CachingHttpHandler(_cache) { InnerHandler = innerHandler };
            return new HttpClient(testHandler) { BaseAddress = new Uri("http://localhost/") };
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

        var handler = new CachingHttpHandler(_cache)
        {
            InnerHandler = new HttpClientHandler { AllowAutoRedirect = false },
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseAddress),
        };
        return client;
    }
}