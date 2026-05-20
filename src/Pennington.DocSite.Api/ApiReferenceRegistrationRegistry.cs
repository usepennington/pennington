namespace Pennington.DocSite.Api;

using System.Collections.Generic;
using System.Linq;

/// <summary>Enumerates every <see cref="ApiReferenceRegistration"/> the application registered. Used by the URL-rewrite middleware to map public prefixes onto the internal routing page.</summary>
public sealed class ApiReferenceRegistrationRegistry
{
    /// <summary>Registrations ordered by longest prefix first, so that more-specific prefixes (e.g. <c>/api/spectre/</c>) match before less-specific siblings (e.g. <c>/api/</c>).</summary>
    public IReadOnlyList<ApiReferenceRegistration> Registrations { get; }

    /// <summary>Initializes the registry with every <see cref="ApiReferenceRegistration"/> registered in DI.</summary>
    public ApiReferenceRegistrationRegistry(IEnumerable<ApiReferenceRegistration> registrations)
    {
        Registrations = registrations
            .OrderByDescending(r => r.RoutePrefix.Length)
            .ToArray();
    }
}