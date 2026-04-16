namespace Pennington.Roslyn.Documentation;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>Enumerates the members of a type resolved from an xmldocid and returns structured descriptors ready for rendering.</summary>
public interface IMemberEnumerator
{
    /// <summary>Enumerates members on the type identified by <paramref name="typeXmlDocId"/> filtered by kind and accessibility, sorted by the requested order.</summary>
    Task<IReadOnlyList<MemberDescriptor>> EnumerateAsync(
        string typeXmlDocId,
        MemberKind kind,
        AccessFilter access,
        MemberOrder order);
}