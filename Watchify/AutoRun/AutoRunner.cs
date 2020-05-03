using System;
using System.Threading.Tasks;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class AutoRunner : ICommand
	{
		public string Name => "run";
		public string Description => "Re-build and run your project when changes are detected";

		
		public ICommand Configure(CommandLineOptions options)
		{
			_options = options;
			return this;
		}

		private CommandLineOptions _options;


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