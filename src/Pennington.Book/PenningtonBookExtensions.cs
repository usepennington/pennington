namespace Pennington.Book;

using System.IO.Abstractions;
using Cli;
using Composition;
using Content;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Navigation;
using Rendering;
using Routing;

/// <summary>Dependency injection and middleware extensions for the Pennington PDF book feature.</summary>
public static class PenningtonBookExtensions
{
    /// <summary>
    /// Adds PDF book generation: a per-locale book per <see cref="BookDefinition"/> (or one whole-site
    /// book when none are configured), served on demand at <c>/pdf/{slug}.pdf</c> in dev and emitted into
    /// the static build. Registers an <see cref="IDownloadLinkProvider"/> a host's chrome can advertise.
    /// </summary>
    public static IServiceCollection AddPenningtonBook(this IServiceCollection services, Action<BookOptions>? configure = null)
    {
        var options = new BookOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Core AddPennington registers this too; TryAdd keeps the package self-sufficient.
        services.TryAddSingleton(TimeProvider.System);

        // Request-path catalog (no Chromium, no projection) for sidebar links.
        services.AddSingleton<IDownloadLinkProvider, BookCatalog>();

        // Process-lifetime browser — the documented connection-pool singleton exception.
        services.AddSingleton<ChromiumBrowserProvider>();

        // File-watched artifact service shared by the emitter and middleware; recreated on content change.
        services.AddFileWatched<BookArtifactService>();

        // Build-time emitter (transient so it captures the current file-watched service).
        services.AddTransient<IContentEmitter, BookContentEmitter>();

        // Read-only `diag books` inspection.
        services.AddSingleton<IDiagCommand, DiagBooksCommand>();

        // Composition collaborators.
        services.AddTransient<BookComposer>();
        services.AddTransient(sp => new AssetInliner(
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IWebHostEnvironment>().WebRootFileProvider,
            sp.GetRequiredService<CanonicalBaseUrl>()));

        return services;
    }

    /// <summary>
    /// Serves book PDFs and live previews in dev mode. A no-op when <c>AddPenningtonBook</c> was not
    /// called. Call after <c>UsePennington</c> / <c>UseDocSite</c>.
    /// </summary>
    public static WebApplication UsePenningtonBook(this WebApplication app)
    {
        if (app.Services.GetService<BookOptions>() is null)
        {
            return app;
        }

        app.UseMiddleware<BookMiddleware>();
        return app;
    }
}
