using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLPlayer.Services;

/// <summary>
/// Canonical release URLs. Single source of truth for every update-related link.
/// </summary>
public static class UpdateUrls
{
    public const string ReleasesPage = "https://github.com/skychenchang/LLPlayer-ChickenRice/releases";
    public const string LatestReleaseApi =
        "https://api.github.com/repos/skychenchang/LLPlayer-ChickenRice/releases/latest";
}

public enum UpdateCheckKind
{
    UpdateAvailable,
    UpToDate,
    NoPublishedRelease,
    Failed
}

/// <param name="LatestVersion">Normalized version, e.g. "0.3.6"; null when unavailable.</param>
/// <param name="RawTag">Original tag_name, e.g. "v0.3.6"; null when unavailable.</param>
/// <param name="ReleaseUrl">Always the releases page (a stable entry point).</param>
public sealed record UpdateCheckResult(
    UpdateCheckKind Kind,
    string? LatestVersion,
    string? RawTag,
    string? ReleaseUrl);

/// <summary>
/// Compares the running version with the latest published GitHub release.
/// Does not touch any player state; safe to call from a background request.
/// </summary>
public static class UpdateChecker
{
    private static readonly Regex VersionInTag = new(@"\d+(?:\.\d+){1,3}", RegexOptions.Compiled);

    /// <summary>
    /// <paramref name="handler"/> / <paramref name="currentVersion"/> exist for
    /// unit tests; production passes only the token and reads <see cref="App.Version"/>.
    /// </summary>
    public static async Task<UpdateCheckResult> CheckAsync(
        CancellationToken token,
        HttpMessageHandler? handler = null,
        string? currentVersion = null)
    {
        try
        {
            using HttpClient client = handler is null
                ? new HttpClient()
                : new HttpClient(handler, disposeHandler: true);
            client.Timeout = TimeSpan.FromSeconds(12);
            // api.github.com rejects requests without a User-Agent (403).
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"LLPlayer/{App.Version}");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

            using HttpResponseMessage response = await client.GetAsync(
                UpdateUrls.LatestReleaseApi,
                HttpCompletionOption.ResponseHeadersRead,
                token);

            // A repository with no published release answers 404: this is a
            // distinct state, not a network/permission failure.
            if (response.StatusCode == HttpStatusCode.NotFound)
                return new(UpdateCheckKind.NoPublishedRelease, null, null, UpdateUrls.ReleasesPage);

            // 403 (rate limit), 429, 5xx -> Failed.
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(token);
            using JsonDocument doc = await JsonDocument.ParseAsync(stream, cancellationToken: token);
            JsonElement root = doc.RootElement;

            bool draft = root.TryGetProperty("draft", out JsonElement draftElement) &&
                         draftElement.ValueKind == JsonValueKind.True;
            bool prerelease = root.TryGetProperty("prerelease", out JsonElement preElement) &&
                              preElement.ValueKind == JsonValueKind.True;
            string tag = root.TryGetProperty("tag_name", out JsonElement tagElement)
                ? tagElement.GetString() ?? string.Empty
                : string.Empty;

            // The endpoint already excludes drafts/pre-releases; keep the guard
            // so a malformed or unexpected payload cannot advertise one.
            if (draft || prerelease || string.IsNullOrWhiteSpace(tag))
                return new(UpdateCheckKind.NoPublishedRelease, null, null, UpdateUrls.ReleasesPage);

            Match match = VersionInTag.Match(tag);
            Version? latest = match.Success && Version.TryParse(match.Value, out Version? parsedLatest)
                ? parsedLatest
                : null;
            Version? current = Version.TryParse(currentVersion ?? App.Version, out Version? parsedCurrent)
                ? parsedCurrent
                : null;

            if (latest is null || current is null)
                return new(UpdateCheckKind.Failed, null, tag, UpdateUrls.ReleasesPage);

            return latest > current
                ? new(UpdateCheckKind.UpdateAvailable, latest.ToString(), tag, UpdateUrls.ReleasesPage)
                : new(UpdateCheckKind.UpToDate, latest.ToString(), tag, UpdateUrls.ReleasesPage);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // A user/app-initiated cancellation is not a failed check.
            throw;
        }
        catch (Exception)
        {
            // Timeout, DNS, TLS, rate limit, malformed JSON -> one clear state.
            return new(UpdateCheckKind.Failed, null, null, UpdateUrls.ReleasesPage);
        }
    }
}
