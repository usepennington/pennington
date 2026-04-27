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

    /// <summary>Initializes the dispatcher with the host's registered <see cref="IServer"/>.</summary>
    public HttpDispatcher(IServer server)
    {
        _server = server;
    }

    /// <inheritdoc/>
    public HttpClient CreateClient()
    {
        if (_server is TestServer testServer)
        {
            // TestServer.CreateClient() returns BaseAddress = http://localhost/.
            // Path-relative URLs ("/foo/bar") resolve against that and are dispatched
            // to the same RequestDelegate Kestrel would have invoked.
            return testServer.CreateClient();
        }

        var addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses;
        if (addresses is null || addresses.Count == 0)
        {
            throw new InvalidOperationException(
                "HttpDispatcher requires either a TestServer or a listening Kestrel host. " +
                "IServerAddressesFeature has no addresses — is the app started yet?");
        }

        // Prefer http:// to avoid dev-cert trust issues when self-fetching from Kestrel.
        var baseAddress = addresses.FirstOrDefault(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            ?? addresses.First();

        var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(baseAddress),
        };
        return client;
    }
}
