using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Serilog.Core;
using Serilog.Events;

namespace Watchify.CommandLine
{
	public class CommandLineOptions
	{
		public ICommand Command { get; set; }
		public DirectoryInfo ProjectDir { get; private set; }
		public bool IsVerboseLoggingEnabled { get; private set; }
		public string LaunchProfileName { get; private set; }


		public CommandLineOptions(ConsoleApplication app, RootCommand rootCommand, LoggingLevelSwitch logLevelSwitch)
		{
			_app = app;
			_rootCommand = rootCommand;
			_logLevelSwitch = logLevelSwitch;
		}

		private readonly ConsoleApplication _app;
		private readonly RootCommand _rootCommand;
		private readonly LoggingLevelSwitch _logLevelSwitch;


		public CommandLineOptions Parse(string[] args)
		{
			var projectDirInput = _app.Option(
				"-p|--project", 
				"Path to directory containing the project you want to run and watch (defaults to current directory)",
				CommandOptionType.SingleValue);

			var isVerboseLoggingEnabled = _app.Option(
				"-v|--verbose",
				"Verbose logging",
				CommandOptionType.NoValue);

			var launchProfile = _app.Option(
				"--launch-profile",
				"The launch profile to use when watching the project for changes",
				CommandOptionType.SingleValue);

			_rootCommand.Configure(this);

			var result = _app.Execute(args);
				
			ProjectDir = GetProjectDir(projectDirInput);
			IsVerboseLoggingEnabled = isVerboseLoggingEnabled.HasValue();
			if (IsVerboseLoggingEnabled)
				_logLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
			LaunchProfileName = launchProfile.Value();

			return result != 0 ? null : this;
		}


		private static DirectoryInfo GetProjectDir(CommandOption projectDirInput)
		{
			DirectoryInfo projectDir;
			if (projectDirInput.HasValue() && projectDirInput.Value().Trim() != ".")
			{
				projectDir = new DirectoryInfo(projectDirInput.Value());
				if (!projectDir.Exists)
				{
					throw new Exception($"Directory not found: {projectDirInput}");
				}
			}
			else
			{
				projectDir = new DirectoryInfo(Environment.CurrentDirectory);
			}

			return projectDir;
		}

	}
}