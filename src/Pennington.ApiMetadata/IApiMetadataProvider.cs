namespace Pennington.ApiMetadata;

using System.Collections.Immutable;
using System.Threading.Tasks;

/// <summary>Backend-neutral source of API documentation metadata. Implementations adapt compiled assemblies, or other metadata sources, to a single contract consumed by the API reference UI.</summary>
public interface IApiMetadataProvider
{
    /// <summary>Returns every documented type the provider knows about, sorted by <see cref="ApiTypeSummary.FullTypeName"/>.</summary>
    Task<ImmutableArray<ApiTypeSummary>> GetTypesAsync();

    /// <summary>Returns full detail for the type identified by <paramref name="uid"/>, or <see langword="null"/> when the type is not known to this provider.</summary>
    Task<ApiTypeDetail?> GetTypeAsync(string uid);

    /// <summary>Returns members of the type identified by <paramref name="typeUid"/> matching the filters.</summary>
    Task<ImmutableArray<ApiMember>> GetMembersAsync(
        string typeUid,
        MemberKind kind,
        AccessFilter access,
        MemberOrder order);

    /// <summary>Returns public extension methods whose first parameter's receiver type has the unqualified name <paramref name="receiverTypeName"/>. Backends that cannot enumerate extensions (e.g. DocFx YAML) return an empty array.</summary>
    Task<ImmutableArray<ExtensionMethodEntry>> GetExtensionMethodsForAsync(string receiverTypeName);

    /// <summary>Returns parsed xmldoc for the type or member identified by <paramref name="uid"/>, or <see cref="ParsedXmlDoc.Empty"/> when the uid is unknown. Used by inline components like <c>&lt;ApiSummary&gt;</c> that target arbitrary symbols.</summary>
    Task<ParsedXmlDoc> GetXmldocAsync(string uid);

    /// <summary>Returns a standalone <see cref="ApiMember"/> for the method/property/field/event identified by <paramref name="uid"/>, or <see langword="null"/> when the uid is unknown or resolves to a type. Used by components that render a single member (e.g. <c>&lt;ApiParameterTable&gt;</c>).</summary>
    Task<ApiMember?> GetMemberAsync(string uid);
}