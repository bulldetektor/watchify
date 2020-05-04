﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class AutoRunner : ICommand
	{
		public string Name => "run";
		public string Description => "Re-build and run your project when changes are detected";

		public AutoRunner(ProcessManager processManager, ILogger<AutoRunner> logger)
		{
			_processManager = processManager;
			_logger = logger;
		}

		private readonly ProcessManager _processManager;
		private readonly ILogger<AutoRunner> _logger;


		public ICommand Configure(CommandLineOptions options)
		{
			_options = options;
			return this;
		}

		private CommandLineOptions _options;


		public int Run()
		{
			_logger.LogDebug($"Running project at {_options.ProjectDir.FullName}");

			_processManager.Start(_options);
			if (_processManager == null)
				return -1;

			Console.CancelKeyPress += (s, a) => _processManager.Stop();

			Console.WriteLine("");
			Console.WriteLine("Watchify is running. Press 'X' to exit...");
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