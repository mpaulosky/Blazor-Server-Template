using Bunit;
using BlazorServerTemplate.Web.Components.Layout;
using BlazorServerTemplate.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerTemplate.Web.Tests.Bunit.Layout;

public class FooterTests : global::Bunit.BunitContext
{
	[Fact]
	public void Render_ShowsCurrentYearCopyright()
	{
		// Arrange
		Services.AddSingleton<IReleaseInfoService>(new StubReleaseInfoService(new ReleaseInfo(null, null)));

		// Act
		var cut = Render<Footer>();

		// Assert
		cut.Markup.Should().Contain(DateTime.UtcNow.Year.ToString());
	}

	[Fact]
	public void Render_WhenReleaseInfoAvailable_ShowsReleaseAndCommit()
	{
		// Arrange
		Services.AddSingleton<IReleaseInfoService>(
			new StubReleaseInfoService(new ReleaseInfo("v1.2.3", "abc1234")));

		// Act
		var cut = Render<Footer>();

		// Assert
		cut.Markup.Should().Contain("v1.2.3");
		cut.Markup.Should().Contain("abc1234");
	}

	private sealed class StubReleaseInfoService(ReleaseInfo releaseInfo) : IReleaseInfoService
	{
		public Task<ReleaseInfo> GetLatestReleaseInfoAsync(CancellationToken cancellationToken = default) =>
			Task.FromResult(releaseInfo);
	}
}
