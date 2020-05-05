using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class AutoRunner : ICommand, IDisposable
	{
		public string Name => "run";
		public string Description => "Re-build and run your project when changes are detected";

		public AutoRunner(
			RunWatchProcessManager runWatchProcessManager,
			ProxyServerProcessManager proxyServerProcessManager,
			LaunchSettingsParser launchSettingsParser,
			ILogger<AutoRunner> logger)
		{
			_runWatchProcessManager = runWatchProcessManager;
			_proxyServerProcessManager = proxyServerProcessManager;
			_launchSettingsParser = launchSettingsParser;
			_logger = logger;
		}

		private readonly RunWatchProcessManager _runWatchProcessManager;
		private readonly ProxyServerProcessManager _proxyServerProcessManager;
		private readonly LaunchSettingsParser _launchSettingsParser;
		private readonly ILogger<AutoRunner> _logger;
		private string _appUrl;


		public ICommand Configure(CommandLineOptions options)
		{
			_options = options;
			return this;
		}

		private CommandLineOptions _options;


		public int Run()
		{
			_logger.LogDebug($"Running project at {_options.ProjectDir.FullName}");

			Task
				.Run(async () =>
				{
					_appUrl = await _launchSettingsParser.GetApplicationUrl(_options.ProjectDir, _options.LaunchProfileName ?? "Development");

					// start dotnet watch
					await _runWatchProcessManager
						.Init(_options)
						.WithLogVerbosity(_options.IsVerboseLoggingEnabled)
						.Start();
					
					if (_runWatchProcessManager == null)
						return -1;

					if (!string.IsNullOrEmpty(_appUrl))
					{
						// start AutoReloadProxy
						await _proxyServerProcessManager
							.Init(_appUrl)
							.ContinueWith(p => p.Result.WithLogVerbosity(_options.IsVerboseLoggingEnabled))
							.ContinueWith(p => p.Result.Start());
					
						if (_proxyServerProcessManager == null)
							return -1;
					}
					return 0;
				})
				.Wait();

			Console.CancelKeyPress += (s, a) => Dispose();

			Console.WriteLine("");
			Console.WriteLine("Watchify is running. Press 'X' to exit...");

			if (!string.IsNullOrEmpty(_appUrl))
			{
				if (Uri.TryCreate(_appUrl, UriKind.RelativeOrAbsolute, out var validUri))
				{
					Console.WriteLine($"Application will be available at {validUri.GetLeftPart(UriPartial.Authority)}");
				}
			}

			Console.WriteLine("");

			while (true)
			{
				var userInput = Console.ReadKey(intercept: true);
				if (userInput.Key == ConsoleKey.X)
				{
					Dispose();
					break;
				}
			}

			Task.Delay(200).Wait();
			return 1;
		}


		public void Dispose()
		{
			_proxyServerProcessManager?.Dispose();
			_runWatchProcessManager?.Dispose();
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