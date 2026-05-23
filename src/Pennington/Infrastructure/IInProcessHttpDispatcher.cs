namespace Pennington.Infrastructure;

/// <summary>
/// Dispatches HTTP requests against the running app's pipeline. Replaces the
/// "self-fetch via Kestrel socket" pattern with a transport that the host can
/// satisfy in-process (via <c>Microsoft.AspNetCore.TestHost.TestServer</c>) or
/// over the wire (via Kestrel's listening address) depending on which
/// <see cref="Microsoft.AspNetCore.Hosting.Server.IServer"/> is registered.
/// <para>
/// Internal services (<c>LlmsTxtService</c>, <c>SearchArtifactService</c>, build
/// crawler) take this instead of <see cref="IHttpClientFactory"/> so they don't
/// need to know whether the host is using TestServer (build / tests) or Kestrel
/// (dev). The middleware pipeline runs identically either way — the only
/// difference is who delivers the bytes.
/// </para>
/// </summary>
public interface IInProcessHttpDispatcher
{
    /// <summary>Returns an <see cref="HttpClient"/> whose requests flow through the running app's pipeline.</summary>
    HttpClient CreateClient();
}