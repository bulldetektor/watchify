using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Watchify.Proxy.Api;

namespace Watchify.AutoRun
{
	public class ProxyServerProcessManager : ProcessManager
	{
		public async Task<ProcessManager> Init(string appUrl)
		{
			_proxyUrl = "http://localhost:3000";
			var args = new[] { "--urls", _proxyUrl };
			_proxyHost = await Proxy.Program
				.CreateHostBuilder(args)
				.StartAsync();

			using var client = new HttpClient { BaseAddress = new Uri("http://localhost:3000") };
			var initContent = new InitializationInput { ProxyTo = appUrl }.ToJsonContent();
			var response = await client.PutAsync("api/proxy/init", initContent);
			var result = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
				WriteError($"Failed to initialize proxy server ({result})");

			return this;
		}
		private string _proxyUrl;
		private IHost _proxyHost;

		protected override string ProcessToStart => _proxyUrl;
		protected override string[] ProcessArgs => new string[0];
		protected override ConsoleColor InfoColor => ConsoleColor.Blue;
		protected override ConsoleColor ErrorColor => ConsoleColor.Red;
		protected override ConsoleColor DebugColor => ConsoleColor.Gray;

		protected override void OnExit(object sender, EventArgs args)
		{
			WriteInfo($"proxy: Proxy server exited");
		}

		protected override void HandleOutput(string message)
		{
			WriteInfo($"proxy: {message}");
		}

		protected override void HandleError(string message)
		{
			WriteInfo($"proxy: {message}");
		}

		public override void Dispose()
		{
			_proxyHost?.StopAsync().Wait(3000);

			base.Dispose();
		}
	}

	
	public static class JsonExtensions
	{
		public static StringContent ToJsonContent(this object obj)
		{
			var json = JsonSerializer.Serialize(obj);//, new JsonSerializerOptions { IgnoreNullValues = true, IgnoreReadOnlyProperties = true });
			return new StringContent(json, Encoding.UTF8, "application/json");
		}
	}
}