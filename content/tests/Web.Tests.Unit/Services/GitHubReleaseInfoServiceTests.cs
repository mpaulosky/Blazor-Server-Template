using BlazorServerTemplate.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlazorServerTemplate.Web.Tests.Unit.Services;

public class GitHubReleaseInfoServiceTests
{
	[Fact]
	public async Task GetLatestReleaseInfoAsync_WhenOwnerAndRepoNotConfigured_ReturnsEmptyReleaseInfo()
	{
		// Arrange
		var configuration = new ConfigurationBuilder().Build();
		var cache = new MemoryCache(new MemoryCacheOptions());
		var httpClient = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
		var sut = new GitHubReleaseInfoService(
			httpClient,
			cache,
			configuration,
			NullLogger<GitHubReleaseInfoService>.Instance);

		// Act
		var result = await sut.GetLatestReleaseInfoAsync(TestContext.Current.CancellationToken);

		// Assert
		result.LatestReleaseTag.Should().BeNull();
		result.LatestCommitSha.Should().BeNull();
	}
}
