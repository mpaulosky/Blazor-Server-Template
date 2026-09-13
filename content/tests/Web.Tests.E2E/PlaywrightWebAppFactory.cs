using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazorServerTemplate.Web.Tests.E2E;

/// <summary>
/// Boots the Web app on a real Kestrel server bound to a random port, so Playwright's browser
/// (a separate process) can navigate to it. A plain WebApplicationFactory only exposes an
/// in-memory TestServer, which a real browser can't reach.
/// </summary>
public sealed class PlaywrightWebAppFactory : WebApplicationFactory<Program>
{
	public string ServerAddress { get; private set; } = string.Empty;

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

		var host = builder.Build();
		host.Start();

		ServerAddress = host.Services
			.GetRequiredService<IServer>()
			.Features.Get<IServerAddressesFeature>()!
			.Addresses.First();

		return host;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseUrls("http://127.0.0.1:0");
		builder.UseEnvironment("Development");
	}
}
