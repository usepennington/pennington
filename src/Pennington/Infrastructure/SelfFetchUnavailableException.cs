namespace Pennington.Infrastructure;

/// <summary>
/// Thrown by <see cref="IInProcessHttpDispatcher.CreateClient"/> when the in-process
/// transport is not ready — the host's <see cref="Microsoft.AspNetCore.Hosting.Server.IServer"/>
/// has not started yet (a <c>TestServer</c> whose application is still null, or a Kestrel host
/// that has not bound a listening address). Distinct from a per-page content failure:
/// site-crawling consumers (notably <see cref="Pennington.Pipeline.SiteProjection"/>) must let
/// this propagate so a partially-built or empty corpus is never cached as if the crawl had
/// completed.
/// </summary>
public sealed class SelfFetchUnavailableException : Exception
{
    /// <summary>Initializes the exception with a message describing why the transport is unavailable.</summary>
    public SelfFetchUnavailableException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes the exception with a message and the underlying cause.</summary>
    public SelfFetchUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
