namespace Pennington.Infrastructure;

/// <summary>
/// Runtime tripwire for the b719d73 deadlock class: a single-flight corpus task (the site
/// projection) awaited from work its own materialization spawned is a task-level circular wait
/// that hangs forever. Two flags mark the dangerous regions so
/// <see cref="Pipeline.SiteProjection"/> can throw a descriptive exception instead of
/// deadlocking:
/// <list type="bullet">
/// <item><see cref="InsideCorpusFetch"/> — this request IS a projection-issued page self-fetch.
/// <see cref="RenderedHtmlFetcher"/> stamps <see cref="HeaderName"/> on every fetch (an
/// <c>AsyncLocal</c> alone would not cross the Kestrel loopback socket in dev), and
/// <c>UsePennington</c> enters the scope when the header is present.</item>
/// <item><see cref="InsideMaterialization"/> — the projection's materialization is on the
/// current async flow; re-entering it (e.g. from a content service's discovery) would await a
/// task that is waiting on the caller.</item>
/// </list>
/// </summary>
public static class CorpusFetchScope
{
    /// <summary>
    /// Header stamped on projection-issued self-fetches. Dev-only surface: a client sending it
    /// manually gets the fail-fast exception on any page whose render path consumes the
    /// projection — which is exactly the bug the exception describes.
    /// </summary>
    public const string HeaderName = "X-Pennington-Corpus-Fetch";

    private static readonly AsyncLocal<bool> CorpusFetch = new();
    private static readonly AsyncLocal<bool> Materialization = new();

    /// <summary>True while processing a request the projection issued to render a page.</summary>
    public static bool InsideCorpusFetch => CorpusFetch.Value;

    /// <summary>True while the projection's corpus materialization is on the current async flow.</summary>
    public static bool InsideMaterialization => Materialization.Value;

    /// <summary>Marks the current async flow as a projection-issued page fetch until disposed.</summary>
    public static IDisposable EnterCorpusFetch() => new Scope(CorpusFetch);

    /// <summary>Marks the current async flow as projection materialization until disposed.</summary>
    public static IDisposable EnterMaterialization() => new Scope(Materialization);

    private sealed class Scope : IDisposable
    {
        private readonly AsyncLocal<bool> _flag;
        private readonly bool _previous;

        public Scope(AsyncLocal<bool> flag)
        {
            _flag = flag;
            _previous = flag.Value;
            flag.Value = true;
        }

        public void Dispose() => _flag.Value = _previous;
    }
}
