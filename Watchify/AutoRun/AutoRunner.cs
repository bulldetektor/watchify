using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class AutoRunner : ICommand
	{
		private readonly CommandLineOptions _options;

		public AutoRunner(CommandLineOptions options)
		{
			_options = options;
		}

		public static void Configure(CommandLineApplication command, CommandLineOptions options)
		{
			command.Description = "Re-build and run your project when changes are detected";
			command.HelpOption(Constants.HelpOptions);

			command.OnExecute(() =>
			{
				options.Command = new AutoRunner(options);

				return 0;
			});
		}

		public int Run()
		{
			Console.WriteLine($"Running project at {_options.ProjectDir.FullName}");

			using var processManager = new ProcessManager();
			processManager.Start(_options.ProjectDir);

			Console.CancelKeyPress += (s, a) => processManager.Stop();

			Console.WriteLine("");
			Console.WriteLine("Press X to exit...");
			Console.WriteLine("");

			while (true)
			{
				var userInput = Console.ReadKey(intercept: true);
				if (userInput.Key == ConsoleKey.X)
				{
					processManager.Stop();
					break;
				}
			}

			Task.Delay(1000).Wait();

			return 1;
		}
	}
}