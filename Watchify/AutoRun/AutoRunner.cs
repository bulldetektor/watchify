using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class AutoRunner : ICommand
	{
		public string Name => "run";
		public string Description => "Re-build and run your project when changes are detected";

		public AutoRunner(ProcessManager processManager, LaunchProfileParser launchProfileParser, ILogger<AutoRunner> logger)
		{
			_processManager = processManager;
			_launchProfileParser = launchProfileParser;
			_logger = logger;
		}

		private readonly ProcessManager _processManager;
		private readonly LaunchProfileParser _launchProfileParser;
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
					_appUrl = await _launchProfileParser.GetApplicationUrl(_options.ProjectDir, _options.LaunchProfileName ?? "Development");

					await _processManager.Start(_options);
					if (_processManager == null)
						return -1;
					return 0;
				})
				.Wait();

			Console.CancelKeyPress += (s, a) => _processManager.Stop();

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
					_processManager.Stop();
					break;
				}
			}

			Task.Delay(200).Wait();
			return 1;
		}

	}
}