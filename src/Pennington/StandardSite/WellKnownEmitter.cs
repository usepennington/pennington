namespace Pennington.StandardSite;

using System.Collections.Immutable;
using System.Text;
using Content;
using Routing;

/// <summary>
/// Bakes the Standard Site verification well-known files into the static build output: the
/// publication AT-URI at <c>/.well-known/site.standard.publication[{path}]</c> and, when enabled,
/// the bare DID at <c>/.well-known/atproto-did</c>. Both are plain-text bodies. Emits nothing when
/// the options are incompletely configured (fail-safe). Transient so it captures current options.
/// </summary>
public sealed class WellKnownEmitter : IContentEmitter
{
    private readonly StandardSiteOptions _options;

    /// <summary>Creates the emitter from the Standard Site options.</summary>
    public WellKnownEmitter(StandardSiteOptions options) => _options = options;

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentToCreate>();
        if (!_options.IsConfigured)
        {
            return Task.FromResult(builder.ToImmutable());
        }

        var publicationUri = AtUri.Build(_options.Did, "site.standard.publication", _options.PublicationRkey);
        var publicationPath = ".well-known/site.standard.publication" + _options.PublicationPath.TrimEnd('/');
        builder.Add(new ContentToCreate(
            new FilePath(publicationPath),
            () => Task.FromResult(Encoding.UTF8.GetBytes(publicationUri)),
            "text/plain"));

        if (_options.EmitAtprotoDid)
        {
            builder.Add(new ContentToCreate(
                new FilePath(".well-known/atproto-did"),
                () => Task.FromResult(Encoding.UTF8.GetBytes(_options.Did)),
                "text/plain"));
        }

        return Task.FromResult(builder.ToImmutable());
    }
}
