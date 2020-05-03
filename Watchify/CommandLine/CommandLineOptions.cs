using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace Watchify.CommandLine
{
	public class CommandLineOptions
	{
		public ICommand Command { get; set; }
		public DirectoryInfo ProjectDir { get; private set; }


		public CommandLineOptions(ConsoleApplication app, RootCommand rootCommand)
		{
			_app = app;
			_rootCommand = rootCommand;
		}

		private readonly ConsoleApplication _app;
		private readonly RootCommand _rootCommand;

		
		public CommandLineOptions Parse(string[] args)
		{
			var projectDirInput = _app.Option(
				"-p|--project", 
				"Path to directory containing the project you want to run and watch (defaults to current directory)",
				CommandOptionType.SingleValue);

			_rootCommand.Configure(this);

			var result = _app.Execute(args);
				
			ProjectDir = GetProjectDir(projectDirInput);

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