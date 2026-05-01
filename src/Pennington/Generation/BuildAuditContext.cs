namespace Pennington.Generation;

using System.Collections.Immutable;
using Content;
using Infrastructure;

/// <summary>Inputs handed to <see cref="IBuildAuditor.AuditAsync"/>.</summary>
/// <param name="Pages">All TOC entries from every registered <see cref="IContentService"/>, post-discovery and locale-aware.</param>
/// <param name="Localization">Configured locales and the default-locale code.</param>
public sealed record BuildAuditContext(
    ImmutableList<ContentTocItem> Pages,
    LocalizationOptions Localization
);
