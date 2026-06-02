namespace BeyondRemoteContentExample;

using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Typed <see cref="HttpClient"/> over the GitHub Releases API. Fetches the public
/// release list for one repository and, on any network failure — offline build,
/// rate limit, timeout — falls back to a committed fixture so the static build
/// still succeeds. Registered with <c>AddHttpClient&lt;GitHubReleasesClient&gt;</c>
/// in <c>Program.cs</c>, where the mandatory User-Agent header and the request
/// timeout are configured.
/// </summary>
public sealed class GitHubReleasesClient
{
    // The repository whose releases drive the site. Swap for your own.
    private const string Owner = "octokit";
    private const string Repo = "octokit.net";

    private readonly HttpClient _http;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<GitHubReleasesClient> _logger;

    /// <summary>Creates the client; <paramref name="http"/> is supplied by the HTTP client factory.</summary>
    public GitHubReleasesClient(
        HttpClient http,
        IWebHostEnvironment environment,
        ILogger<GitHubReleasesClient> logger)
    {
        _http = http;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Fetches the published releases newest-first, dropping drafts. Returns the
    /// bundled fixture when the API is unreachable, slow, or returns nothing.
    /// </summary>
    public async Task<ImmutableList<GitHubRelease>> GetReleasesAsync()
    {
        try
        {
            // The User-Agent header (set in Program.cs) is required — GitHub answers
            // 403 without one. per_page caps the page; a busy repo paginates via the
            // response Link header.
            var releases = await _http.GetFromJsonAsync<List<GitHubRelease>>(
                $"repos/{Owner}/{Repo}/releases?per_page=20", JsonOptions);

            if (releases is { Count: > 0 })
            {
                return [.. releases.Where(r => !r.Draft)];
            }

            _logger.LogWarning("GitHub returned no releases; using the bundled fixture.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Fail open: a slow, unreachable, or rate-limited API must not break the
            // build. To fail the build instead, rethrow here and let it propagate.
            _logger.LogWarning(ex, "GitHub releases fetch failed; using the bundled fixture.");
        }

        return await LoadFixtureAsync();
    }

    private async Task<ImmutableList<GitHubRelease>> LoadFixtureAsync()
    {
        var path = Path.Combine(_environment.ContentRootPath, "fixtures", "github-releases.json");
        await using var stream = File.OpenRead(path);
        var releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, JsonOptions);
        return releases is null ? [] : [.. releases.Where(r => !r.Draft)];
    }

    // GitHub's JSON is snake_case; the policy maps it onto the PascalCase record
    // (tag_name -> TagName, html_url -> HtmlUrl, published_at -> PublishedAt).
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}

/// <summary>The subset of a GitHub release record this site renders.</summary>
/// <param name="TagName">Git tag the release points at (for example <c>v9.1.0</c>).</param>
/// <param name="Name">Display title; may be empty, in which case the tag stands in.</param>
/// <param name="Body">Release notes as GitHub-flavored markdown.</param>
/// <param name="PublishedAt">Publication timestamp, or null for an unpublished draft.</param>
/// <param name="HtmlUrl">Canonical URL of the release on GitHub.</param>
/// <param name="Draft">True for an unpublished draft release.</param>
/// <param name="Prerelease">True when the release is flagged as a pre-release.</param>
public sealed record GitHubRelease(
    string TagName,
    string? Name,
    string? Body,
    DateTimeOffset? PublishedAt,
    string HtmlUrl,
    bool Draft,
    bool Prerelease);
