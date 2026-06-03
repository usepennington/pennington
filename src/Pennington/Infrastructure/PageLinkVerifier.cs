namespace Pennington.Infrastructure;

using Content;
using Generation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Pipeline;

/// <summary>
/// Holds a <see cref="LinkVerificationService"/> built from the current corpus of known
/// routes and copied assets. File-watched so the known-paths set refreshes when content
/// changes. Lets <see cref="PageLinkAuditProcessor"/> verify per-page links without
/// running a corpus-wide rendered audit.
/// </summary>
public sealed class PageLinkVerifier : IFileWatchAware
{
    private readonly AsyncLazy<LinkVerificationService> _verifierLazy;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>Creates the verifier; the underlying <see cref="LinkVerificationService"/> is built lazily on first request.</summary>
    public PageLinkVerifier(
        IEnumerable<IContentService> contentServices,
        EndpointDataSource endpointDataSource,
        OutputOptions outputOptions,
        IWebHostEnvironment environment)
    {
        // includeEmitterOutputs: false — this verifier runs inside the response pipeline,
        // and emitter discovery walks the shared site projection, which kicks off corpus-wide
        // HTTP self-fetches. Those self-fetches would re-enter the per-request audit
        // processor and deadlock on the verifier-build task that's waiting on them. The
        // ISiteProjection xmldoc codifies the same constraint. Build-mode LinkAuditor
        // still includes emitter outputs (consumed via the projection out-of-band) in
        // the full report.
        _verifierLazy = new AsyncLazy<LinkVerificationService>(
            () => LinkVerificationServiceBuilder.BuildAsync(
                contentServices,
                contentEmitters: [],
                endpointDataSource,
                outputOptions,
                includeEmitterOutputs: false,
                environment.WebRootFileProvider));
    }

    /// <summary>Returns the current verifier; rebuilds on first access after a file change.</summary>
    public Task<LinkVerificationService> GetVerifierAsync() => _verifierLazy.Task;
}
