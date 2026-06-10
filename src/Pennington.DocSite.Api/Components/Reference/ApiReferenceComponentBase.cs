namespace Pennington.DocSite.Api.Components.Reference;

using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.ApiMetadata;
using Pennington.Diagnostics;
using Pennington.Infrastructure;

/// <summary>
/// Shared base for the API-reference Razor components. Centralizes the service injection,
/// request-scoped <see cref="DiagnosticContext"/> lookup, keyed-provider resolution, and a single
/// guarded fetch path so every component handles a provider failure the same way — a diagnostic plus
/// inline error HTML — instead of letting the exception escape the render.
/// </summary>
public abstract class ApiReferenceComponentBase : ComponentBase
{
    /// <summary>Root service provider, used to resolve the keyed <see cref="IApiMetadataProvider"/>.</summary>
    [Inject] protected IServiceProvider Services { get; set; } = default!;

    /// <summary>Renders parsed xmldoc nodes to HTML for the derived components.</summary>
    [Inject] protected IXmlDocHtmlRenderer Renderer { get; set; } = default!;

    /// <summary>Accessor used to reach the request-scoped <see cref="DiagnosticContext"/>.</summary>
    [Inject] protected IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    /// <summary>Explicit provider name set on the component; wins over the cascading value.</summary>
    [Parameter] public string? Source { get; set; }

    /// <summary>Provider name cascaded from the enclosing API-reference page.</summary>
    [CascadingParameter(Name = "ApiSource")] public string? CascadingSource { get; set; }

    /// <summary>Request-scoped diagnostics sink, or <see langword="null"/> outside a request.</summary>
    protected DiagnosticContext? Diagnostics =>
        HttpContextAccessor.HttpContext?.RequestServices.GetService<DiagnosticContext>();

    /// <summary>
    /// Resolves the keyed provider and runs <paramref name="fetch"/> synchronously, capturing any
    /// exception — including a missing provider registration — into a diagnostic plus inline error HTML.
    /// </summary>
    /// <typeparam name="T">Result type produced by the fetch.</typeparam>
    /// <param name="componentTag">Component element name used in the diagnostic message (e.g. <c>ApiSummary</c>).</param>
    /// <param name="subject">The xmldocid or receiver the fetch targets, echoed in the error.</param>
    /// <param name="fetch">Provider call to run.</param>
    protected ApiFetch<T> Fetch<T>(string componentTag, string subject, Func<IApiMetadataProvider, Task<T>> fetch)
    {
        try
        {
            var provider = ApiSourceResolver.ResolveProvider(Services, Source, CascadingSource);
            return new ApiFetch<T>(true, AsyncHelpers.RunSync(() => fetch(provider)), null);
        }
        catch (Exception ex)
        {
            Diagnostics?.AddError($"<{componentTag}> resolution failed for {subject}: {ex.Message}");
            return new ApiFetch<T>(
                false,
                default!,
                $"Resolution failed for <code>{WebUtility.HtmlEncode(subject)}</code>: {WebUtility.HtmlEncode(ex.Message)}");
        }
    }

    /// <summary>
    /// Records the standard "missing required attribute" diagnostic and returns the matching inline
    /// error HTML for a component whose required attribute was left blank.
    /// </summary>
    /// <param name="componentTag">Component element name (e.g. <c>ApiSummary</c>).</param>
    /// <param name="attributeName">The required attribute that is missing (e.g. <c>XmlDocId</c>).</param>
    protected string MissingAttributeError(string componentTag, string attributeName)
    {
        Diagnostics?.AddError($"<{componentTag}> missing {attributeName}");
        return $"&lt;{componentTag}&gt; missing {attributeName}";
    }

    /// <summary>Wraps inline error HTML in the standard <c>diag-error</c> block, for components that render a raw HTML string.</summary>
    /// <param name="innerHtml">Inner error HTML from <see cref="Fetch{T}"/> or <see cref="MissingAttributeError"/>.</param>
    protected static string DiagError(string innerHtml) => $"<div class=\"diag-error\">{innerHtml}</div>";
}

/// <summary>Result of <see cref="ApiReferenceComponentBase.Fetch{T}"/>: a success flag with either the value or inline error HTML.</summary>
/// <typeparam name="T">Result type produced by the fetch.</typeparam>
/// <param name="Ok">Whether the fetch succeeded.</param>
/// <param name="Value">The fetched value when <see cref="Ok"/> is <see langword="true"/>; otherwise the default.</param>
/// <param name="ErrorHtml">Inline error HTML when <see cref="Ok"/> is <see langword="false"/>; otherwise <see langword="null"/>.</param>
public readonly record struct ApiFetch<T>(bool Ok, T Value, string? ErrorHtml);
