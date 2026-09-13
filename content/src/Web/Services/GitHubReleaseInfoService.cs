using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorServerTemplate.Web.Services;

public sealed class GitHubReleaseInfoService(
	HttpClient httpClient,
	IMemoryCache cache,
	IConfiguration configuration,
	ILogger<GitHubReleaseInfoService> logger) : IReleaseInfoService
{
	private const string CacheKey = "github-release-info";
	private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

	public async Task<ReleaseInfo> GetLatestReleaseInfoAsync(CancellationToken cancellationToken = default)
	{
		if (cache.TryGetValue(CacheKey, out ReleaseInfo? cached) && cached is not null)
		{
			return cached;
		}

		var owner = configuration["GitHub:Owner"];
		var repo = configuration["GitHub:Repo"];

		if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
		{
			return new ReleaseInfo(null, null);
		}

		var info = await FetchAsync(owner, repo, cancellationToken);
		cache.Set(CacheKey, info, CacheDuration);
		return info;
	}

	private async Task<ReleaseInfo> FetchAsync(string owner, string repo, CancellationToken cancellationToken)
	{
		try
		{
			var releaseTask = httpClient.GetFromJsonAsync<GitHubRelease>(
				$"repos/{owner}/{repo}/releases/latest", cancellationToken);
			var commitsTask = httpClient.GetFromJsonAsync<GitHubCommit[]>(
				$"repos/{owner}/{repo}/commits?per_page=1", cancellationToken);

			await Task.WhenAll(releaseTask, commitsTask);

			var tag = (await releaseTask)?.TagName;
			var sha = (await commitsTask)?.FirstOrDefault()?.Sha;

			return new ReleaseInfo(tag, sha is null ? null : sha[..Math.Min(7, sha.Length)]);
		}
		catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or TaskCanceledException)
		{
			logger.LogWarning(ex, "Unable to fetch release info from GitHub for {Owner}/{Repo}", owner, repo);
			return new ReleaseInfo(null, null);
		}
	}

	private sealed record GitHubRelease([property: JsonPropertyName("tag_name")] string? TagName);

	private sealed record GitHubCommit([property: JsonPropertyName("sha")] string? Sha);
}
