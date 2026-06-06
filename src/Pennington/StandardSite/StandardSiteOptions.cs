namespace Pennington.StandardSite;

using Content;
using FrontMatter;

/// <summary>
/// Configures the Standard Site (AT Protocol long-form publishing) integration. The verification
/// surface — the well-known files and per-page <c>site.standard.*</c> head links — needs only
/// <see cref="Did"/> and <see cref="PublicationRkey"/> and writes nothing to any PDS. When either is
/// blank the feature emits nothing (fail-safe). Records themselves are authored out-of-band (an
/// editor such as standard.horse); this engine only publishes the proof that points at them.
/// </summary>
public sealed record StandardSiteOptions
{
    /// <summary>Author/owner DID, e.g. <c>did:plc:abc123</c>. Required for any output.</summary>
    public required string Did { get; init; }

    /// <summary>
    /// Record key of the <c>site.standard.publication</c> record in the author's repo. The
    /// publication AT-URI is <c>at://{Did}/site.standard.publication/{PublicationRkey}</c>.
    /// </summary>
    public required string PublicationRkey { get; init; }

    /// <summary>
    /// Path the publication is served under, with a leading slash and no trailing slash
    /// (default <c>""</c> = domain root). Appended to the publication well-known suffix.
    /// </summary>
    public string PublicationPath { get; init; } = "";

    /// <summary>
    /// When true, also emit <c>/.well-known/atproto-did</c> so the site's domain doubles as the
    /// author's atproto handle. Default <c>false</c>: this is a stronger, separate claim than
    /// publication verification, and a mismatched DID can break an existing domain handle.
    /// </summary>
    public bool EmitAtprotoDid { get; init; }

    /// <summary>
    /// When true (the default), emit the site-wide <c>&lt;link rel="site.standard.publication"&gt;</c>
    /// discovery hint in every page head — what a Bluesky card reader looks for.
    /// </summary>
    public bool EmitPublicationLink { get; init; } = true;

    /// <summary>
    /// Resolves a page's <c>site.standard.document</c> record key from its content record. Defaults
    /// to reading <see cref="IStandardSiteDocument.AtprotoRkey"/> off the front matter, so hosts that
    /// don't implement that capability can map the rkey from their own metadata instead.
    /// </summary>
    public Func<ContentRecord, string?> DocumentRkeyResolver { get; init; }
        = static record => (record.Metadata as IStandardSiteDocument)?.AtprotoRkey;

    /// <summary>True when both <see cref="Did"/> and <see cref="PublicationRkey"/> are set.</summary>
    public bool IsConfigured => !string.IsNullOrEmpty(Did) && !string.IsNullOrEmpty(PublicationRkey);
}
