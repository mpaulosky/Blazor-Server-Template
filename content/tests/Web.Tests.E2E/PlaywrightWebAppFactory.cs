using System.Diagnostics;
using System.Net.Sockets;

namespace BlazorServerTemplate.Web.Tests.E2E;

/// <summary>
/// Boots the Web app as a real, separate process bound to a free port, so Playwright's browser
/// (itself a separate process) can navigate to it. WebApplicationFactory's Server/Services
/// accessors assume an in-memory TestServer and throw when the host runs real Kestrel instead,
/// so this drives the actual built app the same way `dotnet run` would.
/// </summary>
public sealed class PlaywrightWebAppFactory : IAsyncLifetime
{
	private Process? _process;

	public string ServerAddress { get; private set; } = string.Empty;

	public async ValueTask InitializeAsync()
	{
		var webAssemblyPath = typeof(Program).Assembly.Location;
		var port = GetFreeTcpPort();
		ServerAddress = $"http://127.0.0.1:{port}";

		_process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $"\"{webAssemblyPath}\"",
				WorkingDirectory = Path.GetDirectoryName(webAssemblyPath),
				UseShellExecute = false,
				EnvironmentVariables =
				{
					["ASPNETCORE_URLS"] = ServerAddress,
					["ASPNETCORE_ENVIRONMENT"] = "Development"
				}
			}
		};
		_process.Start();

		await WaitUntilReadyAsync();
	}

	public async ValueTask DisposeAsync()
	{
		if (_process is null)
		{
			return;
		}

		if (!_process.HasExited)
		{
			_process.Kill(entireProcessTree: true);
			await _process.WaitForExitAsync();
		}

		_process.Dispose();
	}

	private async Task WaitUntilReadyAsync()
	{
		using var httpClient = new HttpClient();
		var deadline = DateTime.UtcNow.AddSeconds(30);

		while (DateTime.UtcNow < deadline)
		{
			try
			{
				var response = await httpClient.GetAsync(ServerAddress);
				if (response.IsSuccessStatusCode)
				{
					return;
				}
			}
			catch (HttpRequestException)
			{
				// Server isn't accepting connections yet; keep polling.
			}

			await Task.Delay(200);
		}

		throw new TimeoutException($"The Web app did not start listening on {ServerAddress} within 30 seconds.");
	}

	private static int GetFreeTcpPort()
	{
		using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
		listener.Start();
		var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
		listener.Stop();
		return port;
	}
}
