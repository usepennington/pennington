namespace Pennington.Artifacts;

/// <summary>The bytes and content type for one resolved artifact request.</summary>
/// <param name="Bytes">Response body, served in dev and written verbatim by the static build.</param>
/// <param name="ContentType">Full content-type header value (e.g. <c>application/json; charset=utf-8</c>).</param>
public sealed record ArtifactContent(byte[] Bytes, string ContentType);
