namespace Pennington.DocSite.Api.Components.Reference;

using Pennington.ApiMetadata;

internal static class MemberOptionParser
{
    public static MemberKind ParseKind(string value) => value.Trim().ToLowerInvariant() switch
    {
        "properties" => MemberKind.Properties,
        "fields" => MemberKind.Fields,
        "methods" => MemberKind.Methods,
        "constructors" => MemberKind.Constructors,
        "events" => MemberKind.Events,
        "all" => MemberKind.All,
        _ => MemberKind.Properties,
    };

    public static AccessFilter ParseAccess(string value) => value.Trim().ToLowerInvariant() switch
    {
        "public" => AccessFilter.Public,
        "protected" => AccessFilter.Protected,
        "all" or "public+protected" or "publicandprotected" => AccessFilter.PublicAndProtected,
        _ => AccessFilter.Public,
    };

    public static MemberOrder ParseOrder(string value) => value.Trim().ToLowerInvariant() switch
    {
        "declaration" => MemberOrder.Declaration,
        _ => MemberOrder.Alphabetical,
    };
}
