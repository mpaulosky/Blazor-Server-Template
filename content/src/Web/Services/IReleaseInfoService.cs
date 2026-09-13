namespace BlazorServerTemplate.Web.Services;

public interface IReleaseInfoService
{
	Task<ReleaseInfo> GetLatestReleaseInfoAsync(CancellationToken cancellationToken = default);
}

public sealed record ReleaseInfo(string? LatestReleaseTag, string? LatestCommitSha);
