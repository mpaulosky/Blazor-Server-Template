using Microsoft.Playwright;

namespace BlazorServerTemplate.Web.Tests.E2E;

public class HomePageTests : IClassFixture<PlaywrightWebAppFactory>, IAsyncLifetime
{
	private readonly PlaywrightWebAppFactory _factory;
	private IPlaywright _playwright = null!;
	private IBrowser _browser = null!;

	public HomePageTests(PlaywrightWebAppFactory factory)
	{
		_factory = factory;
	}

	public async ValueTask InitializeAsync()
	{
		_playwright = await Playwright.CreateAsync();
		_browser = await _playwright.Chromium.LaunchAsync();
	}

	public async ValueTask DisposeAsync()
	{
		await _browser.DisposeAsync();
		_playwright.Dispose();
	}

	[Fact]
	public async Task Home_DisplaysWelcomeHeading()
	{
		// Arrange
		var page = await _browser.NewPageAsync();

		// Act
		await page.GotoAsync(_factory.ServerAddress);

		// Assert
		var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Welcome to BlazorServerTemplate" });
		(await heading.IsVisibleAsync()).Should().BeTrue();
	}

	[Fact]
	public async Task Home_AnonymousVisitor_SeesLogInLink()
	{
		// Arrange
		var page = await _browser.NewPageAsync();

		// Act
		await page.GotoAsync(_factory.ServerAddress);

		// Assert
		var loginLink = page.GetByRole(AriaRole.Link, new() { Name = "Log in" });
		(await loginLink.IsVisibleAsync()).Should().BeTrue();
	}
}
