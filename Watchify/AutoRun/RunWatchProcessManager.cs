using System;
using System.IO;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class RunWatchProcessManager : ProcessManager
	{
		public ProcessManager Init(CommandLineOptions options)
		{
			_projectDir = options.ProjectDir;
			return this;
		}
		private DirectoryInfo _projectDir;

		protected override string ProcessToStart => "dotnet";

		protected override string[] ProcessArgs => new[]
		{
			"watch",
			"--verbose",
			"--project",
			_projectDir.FullName,
			"run",
			"-- --Logging:LogLevel:Default=Debug"
		};

		protected override ConsoleColor InfoColor => ConsoleColor.DarkYellow;
		protected override ConsoleColor DebugColor => ConsoleColor.DarkGray;
		protected override ConsoleColor ErrorColor => ConsoleColor.DarkRed;

		protected override void OnExit(object sender, EventArgs args)
		{
			ShowToast("Watchify exited");
		}

		private bool _hasReceivedFirstOutput;
		private bool _isRestarting;
		private bool _isStarting;

		protected override void HandleOutput(string output)
		{
			if (output == null)
				return;

			if (output.StartsWith("watch") && !_isRestarting)
			{
				if (!_hasReceivedFirstOutput)
				{
					WriteInfo("Starting up...");
					WriteDebug($"Running (process: {RunningProcess.Id})");
					_hasReceivedFirstOutput = true;
				}

				switch (output.ToLowerInvariant().Trim())
				{
					case "watch : started":
						_isStarting = true;
						break;
					case "watch : exited":
						ShowToast("Watchify: Restarting...");
						WriteInfo("Restarting");
						_isRestarting = true;
						break;
				}
			}
			else if (_isStarting || _isRestarting)
			{
				if (output.StartsWith("info:"))
				{
					ShowToast("Watchify: Ready!");
					WriteInfo("Ready!");
					_isRestarting = false;
					_isStarting = false;
				}
			}

			WriteDebug(output);
		}

		protected override void HandleError(string error)
		{
			if (error?.ToLowerInvariant().StartsWith("watch : exited with error code") ?? false)
			{
				ShowToast("Watchify: Error building and running the app");
			}

			WriteError(error);
		}

	}
}